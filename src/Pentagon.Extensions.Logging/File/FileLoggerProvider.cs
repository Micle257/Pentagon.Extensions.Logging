// -----------------------------------------------------------------------
//  <copyright file="FileLoggerProvider.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Logging.File
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;

    internal class FileLoggerOptionsSetup : ConfigureFromConfigurationOptions<FileLoggerOptions>
    {
        public FileLoggerOptionsSetup(ILoggerProviderConfiguration<FileLoggerProvider> providerConfiguration)
                : base(providerConfiguration.Configuration)
        {
        }
    }

    public interface IFileLoggerSettings
    {
        bool IncludeScopes { get; }

        IChangeToken ChangeToken { get; }

        bool TryGetSwitch(string name, out LogLevel level);

        IFileLoggerSettings Reload();
    }

    public class FileLoggerSettings : IFileLoggerSettings
    {
        public IChangeToken ChangeToken { get; set; }

        public bool IncludeScopes { get; set; }

        public bool DisableColors { get; set; }

        public IDictionary<string, LogLevel> Switches { get; set; } = new Dictionary<string, LogLevel>();

        public IFileLoggerSettings Reload()
        {
            return this;
        }

        public bool TryGetSwitch(string name, out LogLevel level)
        {
            return Switches.TryGetValue(name, out level);
        }
    }

    public class FileLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        readonly IFileAsyncWriter _fileWriter;
        readonly IOptionsMonitor<FileLoggerOptions> _options;
        private readonly Func<string, LogLevel, bool> _filter;
        private bool _includeScopes;
        private IExternalScopeProvider _scopeProvider;
        private IFileLoggerSettings _settings;

        readonly ConcurrentDictionary<string, FileLogger> _loggers = new ConcurrentDictionary<string, FileLogger>();

        readonly IDisposable _optionsReloadToken;

        public FileLoggerProvider(Func<string, LogLevel, bool> filter, bool includeScopes)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _filter = filter;
            _includeScopes = includeScopes;
            _fileWriter = new FileAsyncWriter(options);
        }

        public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            _fileWriter = new FileAsyncWriter(options);
            _options = options;
            _filter = (s, level) => true;
            _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
            ReloadLoggerOptions(options.CurrentValue);
        }

        public FileLoggerProvider(IFileLoggerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _settings = settings;

            if (_settings.ChangeToken != null)
            {
                _settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
            }
        }

        void OnConfigurationReload(object obj)
        {
            try
            {
                _settings = _settings.Reload();

                _includeScopes = _settings?.IncludeScopes ?? false;

                var scopeProvider = GetScopeProvider();
                foreach (var logger in _loggers.Values)
                {
                    logger.Filter = GetFilter(logger.Name, _settings);
                    logger.ScopeProvider = scopeProvider;
                }
            }
            catch (Exception ex)
            {
               // System.Console.WriteLine($"Error while loading configuration changes.{Environment.NewLine}{ex}");
            }
            finally
            {
                if (_settings?.ChangeToken != null)
                {
                    _settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
                }
            }
        }

        Func<string, LogLevel, bool> GetFilter(string name, IFileLoggerSettings settings)
        {
            if (_filter != null)
            {
                return _filter;
            }

            if (settings != null)
            {
                foreach (var prefix in GetKeyPrefixes(name))
                {
                    LogLevel level;
                    if (settings.TryGetSwitch(prefix, out level))
                    {
                        return (n, l) => l >= level;
                    }
                }
            }

            return (s, level) => false;
        }

        IEnumerable<string> GetKeyPrefixes(string name)
        {
            while (!string.IsNullOrEmpty(name))
            {
                yield return name;
                var lastIndexOfDot = name.LastIndexOf('.');
                if (lastIndexOfDot == -1)
                {
                    yield return "Default";
                    break;
                }
                name = name.Substring(0, lastIndexOfDot);
            }
        }

        void ReloadLoggerOptions(FileLoggerOptions options)
        {
            _includeScopes = options.IncludeScopes;

            var scopeProvider = GetScopeProvider();
            foreach (var logger in _loggers.Values)
            {
                logger.ScopeProvider = scopeProvider;
                logger.IncludeCategory = options.IncludeCategory;
                logger.IncludeTimeStamp = options.IncludeTimeStamp;
                logger.IndentMultiLinesFormat = options.IndentMultiLinesFormat;
            }
        }
        
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
        }

        FileLogger CreateLoggerImplementation(string name)
        {
            var includeScopes = _settings?.IncludeScopes ?? _includeScopes;

            return new FileLogger(name,GetFilter(name, _settings),includeScopes? _scopeProvider: null,_fileWriter);
        }

        IExternalScopeProvider GetScopeProvider()
        {
            if (_includeScopes && _scopeProvider == null)
            {
                _scopeProvider = new LoggerExternalScopeProvider();
            }
            return _includeScopes ? _scopeProvider : null;
        }

        /// <inheritdoc />
        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public void Dispose()
        {
            _optionsReloadToken?.Dispose();
            _fileWriter?.Dispose();
        }
    }
}