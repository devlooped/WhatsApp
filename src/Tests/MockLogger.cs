using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Devlooped.WhatsApp;

public class MockLogger(ITestOutputHelper? output = default) : ILoggerFactory
{
    public static ILogger<T> Create<T>(ITestOutputHelper? output = default) => new MockLoggerImpl<T>(output);

    public void AddProvider(ILoggerProvider provider) => throw new NotImplementedException();
    public ILogger CreateLogger(string categoryName) => new MockLoggerImpl(output);
    void IDisposable.Dispose() { }

    class MockLoggerImpl(ITestOutputHelper? output) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => output?.WriteLine(formatter(state, exception));
    }

    class MockLoggerImpl<T>(ITestOutputHelper? output) : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => output?.WriteLine(formatter(state, exception));
    }
}
