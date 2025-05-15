using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

public static class LoggerFactoryExtensions
{
    public static ILoggerFactory AsLoggerFactory(this ITestOutputHelper output) => new LoggerFactory(output);
}

public class LoggerFactory(ITestOutputHelper output) : ILoggerFactory
{
    public ILogger CreateLogger(string categoryName) => new TestOutputLogger(output, categoryName);
    public void AddProvider(ILoggerProvider provider) { }
    public void Dispose() { }

    // create ilogger implementation over testoutputhelper
    public class TestOutputLogger(ITestOutputHelper output, string categoryName) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));
            if (state == null) throw new ArgumentNullException(nameof(state));
            output.WriteLine($"{logLevel}: {categoryName}: {formatter(state, exception)}");
        }
    }
}
