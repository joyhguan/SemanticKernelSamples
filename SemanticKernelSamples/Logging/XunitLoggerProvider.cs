using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace SemanticKernelSamples.Logging
{
    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _output;
        private readonly LogLevel _minLogLevel;

        public XunitLoggerProvider(ITestOutputHelper output, LogLevel minLogLevel)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _minLogLevel = minLogLevel;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(categoryName, _output, _minLogLevel);
        }

        public void Dispose()
        { }
    }
}