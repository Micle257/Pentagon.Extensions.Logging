namespace Pentagon.Extensions.Logging.File {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;

    public class FileAsyncWriter : IFileAsyncWriter, IDisposable
    {
        FileLoggerOptions _options;
        readonly Task _task;
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        readonly BlockingCollection<(DateTimeOffset TimeStamp, string Message)> _messages =
                new BlockingCollection<(DateTimeOffset TimeStamp, string Message)>(new ConcurrentQueue<(DateTimeOffset TimeStamp, string Message)>());

        readonly IList<(DateTimeOffset TimeStamp, string Message)> _currentQueue = new List<(DateTimeOffset TimeStamp, string Message)>();

        IDictionary<DateTime, IList<int>> _fileVersionMap = new ConcurrentDictionary<DateTime, IList<int>>();
        readonly IDisposable _optionsReloadToken;

        public FileAsyncWriter(IOptionsMonitor<FileLoggerOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
            ReloadLoggerOptions(options.CurrentValue);

            _task = Task.Factory.StartNew(ProccessQueueAsync,
                                          CancellationToken.None,
                                          TaskCreationOptions.LongRunning,
                                          TaskScheduler.Default);
        }

        public void AddMessage(DateTimeOffset time, string message)
        {
            if (!_messages.IsAddingCompleted)
                _messages.Add((time, message), _cancellationTokenSource.Token);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _task?.Dispose();
            _cancellationTokenSource?.Dispose();
            _messages?.Dispose();
            _optionsReloadToken?.Dispose();
        }

        protected virtual async Task WriteMessagesAsync(IEnumerable<(DateTimeOffset TimeStamp, string Message)> messageStates, CancellationToken cancellationToken = default(CancellationToken))
        {
            Directory.CreateDirectory(_options.LogDirectory);

            foreach (var dayMessage in messageStates.GroupBy(a => a.TimeStamp.Date))
            {
                var name = GetFileFullPath(dayMessage.Key, _options.LogDirectory);

                if (_options.GroupDirectoryTreeFormat != null)
                {
                    var directoryTree = dayMessage.Key.ToString(_options.GroupDirectoryTreeFormat.Replace(oldValue: "\\", newValue: "/")).Replace(oldValue: "/", newValue: "\\");
                    var path = Path.Combine(_options.LogDirectory, directoryTree);
                    Directory.CreateDirectory(path);

                    name = GetFileFullPath(dayMessage.Key, path);
                }

                //var fileInfo = new FileInfo(name);

                //if (_options.MaxFileSize > 0 && fileInfo.Exists && fileInfo.Length > _options.MaxFileSize)
                //{
                //    if (_fileVersionMap.TryGetValue(dayMessage.Key, out var id))
                //    {
                //        if (id == null)
                //            _fileVersionMap[dayMessage.Key] = new List<int> { 0 };

                //        var newid = id.Max() + 1;

                //    }
                //    else
                //    {
                //        _fileVersionMap.Add(dayMessage.Key);
                //    }
                //}

                using (var streamWriter = File.AppendText(name))
                {
                    foreach (var item in dayMessage)
                        await streamWriter.WriteAsync(item.Message).ConfigureAwait(false);
                }
            }
        }

        void ReloadLoggerOptions(FileLoggerOptions options)
        {
            _options = options;
        }

        async Task ProccessQueueAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                while (_messages.TryTake(out var state))
                    _currentQueue.Add(state);

                await WriteMessagesAsync(_currentQueue, _cancellationTokenSource.Token).ConfigureAwait(false);
                _currentQueue.Clear();

                await Task.Delay(_options.LogFileWriteInterval).ConfigureAwait(false);
            }
        }

        string GetFileFullPath(DateTime date, string path) => Path.Combine(path, $"log_{date.ToString(format: "yyyyMMdd")}.txt");
    }
}