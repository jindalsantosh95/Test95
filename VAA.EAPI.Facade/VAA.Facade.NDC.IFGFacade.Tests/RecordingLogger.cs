using System;
using Microsoft.Extensions.Logging;

namespace VAA.Facade.NDC.IFGFacade.Tests
{
    internal sealed class RecordingLogger<T> : ILogger<T>
    {
        public LogLevel? LastLevel { get; private set; }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LastLevel = logLevel;
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
