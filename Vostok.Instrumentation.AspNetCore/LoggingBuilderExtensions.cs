using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Vostok.Logging;
using LogLevel = Vostok.Logging.LogLevel;

namespace Vostok.Instrumentation.AspNetCore
{
    public static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder AddVostok(this ILoggingBuilder builder, ILog log)
        {
            return builder.AddProvider(new LoggerProvider(log));
        }

        private class LoggerProvider : ILoggerProvider
        {
            private readonly ILog log;

            public LoggerProvider(ILog log)
            {
                this.log = log;
            }

            public void Dispose()
            {
            }

            public ILogger CreateLogger(string categoryName)
            {
                return new LoggerAdapter(string.IsNullOrEmpty(categoryName) ? log : log.ForContext(categoryName));
            }
        }

        private class LoggerAdapter : ILogger
        {
            private readonly ILog log;

            public LoggerAdapter(ILog log)
            {
                this.log = log;
            }

            public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!TranslateLogLevel(logLevel, out var vostokLogLevel) || !log.IsEnabledFor(vostokLogLevel))
                    return;

                var messageTemplate = formatter != null 
                    ? formatter(state, exception) 
                    : ReferenceEquals(state, null)
                        ? typeof(TState).FullName
                        : Convert.ToString(state);

                var logEvent = new LogEvent(vostokLogLevel, exception, messageTemplate, Array.Empty<object>());
                if (state is IEnumerable<KeyValuePair<string, object>> kvps)
                {
                    foreach (var kvp in kvps)
                    {
                        if (kvp.Key == "{OriginalFormat}")
                            continue;
                        logEvent.AddPropertyIfAbsent(kvp.Key, kvp.Value);
                    }
                }
                log.Log(logEvent);
            }

            public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
            {
                return TranslateLogLevel(logLevel, out var vostokLogLevel) && log.IsEnabledFor(vostokLogLevel);
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                // TODO (spaceorc, 14.02.2018): steal scope implementation from SerilogLogger
                return new Scope();
            }

            private static bool TranslateLogLevel(Microsoft.Extensions.Logging.LogLevel logLevel, out LogLevel vostokLogLevel)
            {
                switch (logLevel)
                {
                    case Microsoft.Extensions.Logging.LogLevel.Trace:
                        vostokLogLevel = LogLevel.Trace;
                        return true;
                    case Microsoft.Extensions.Logging.LogLevel.Debug:
                        vostokLogLevel = LogLevel.Debug;
                        return true;
                    case Microsoft.Extensions.Logging.LogLevel.Information:
                        vostokLogLevel = LogLevel.Info;
                        return true;
                    case Microsoft.Extensions.Logging.LogLevel.Warning:
                        vostokLogLevel = LogLevel.Warn;
                        return true;
                    case Microsoft.Extensions.Logging.LogLevel.Error:
                        vostokLogLevel = LogLevel.Error;
                        return true;
                    case Microsoft.Extensions.Logging.LogLevel.Critical:
                        vostokLogLevel = LogLevel.Fatal;
                        return true;
                    default:
                        vostokLogLevel = default(LogLevel);
                        return false;
                }
            }

            private class Scope : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}