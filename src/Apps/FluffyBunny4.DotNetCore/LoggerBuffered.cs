using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace FluffyBunny4.DotNetCore
{
    public class LoggerBuffered : ILogger
    {
        class Entry
        {
            public LogLevel _logLevel;
            public EventId _eventId;
            public string _message;
        }
        LogLevel _minLogLevel;
        List<Entry> _buffer;
        public LoggerBuffered(LogLevel minLogLevel)
        {
            _minLogLevel = minLogLevel;
            _buffer = new List<Entry>();
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _minLogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                var str = formatter(state, exception);
                _buffer.Add(new Entry { _logLevel = logLevel, _eventId = eventId, _message = str });
            }
        }
        public void CopyToLogger(ILogger logger)
        {
            foreach (var entry in _buffer)
            {
                logger.Log(entry._logLevel, entry._eventId, entry._message);
            }
            _buffer.Clear();
        }
    }
}
