using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace SemanticKernelSamples.Logging
{
    public class XunitLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ITestOutputHelper _output;
        private readonly LogLevel _minLogLevel;

        public XunitLogger(string categoryName, ITestOutputHelper output, LogLevel minLogLevel)
        {
            _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _minLogLevel = minLogLevel;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLogLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception == null) return;

            var logEntry = $"[{logLevel} - {_categoryName}]: {message}";
            if (exception != null)
            {
                logEntry = $"{logEntry}{Environment.NewLine}{exception}";
            }

            _output.WriteLine(logEntry);
        }
    }
}