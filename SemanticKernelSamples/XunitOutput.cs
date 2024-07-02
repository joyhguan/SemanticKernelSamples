using Xunit.Abstractions;

namespace SemanticKernelSamples
{
    public interface IXunitOutput
    {
        void WriteLine(string message);

        void WriteLine(object? target = null);

        void WriteLine(string? format, params object?[] args);
    }

    internal class XunitOutput(ITestOutputHelper output) : IXunitOutput
    {
        public void WriteLine(string message)
            => output.WriteLine(message);

        public void WriteLine(object? target = null) =>
            output.WriteLine(target is null ? string.Empty : target.ToString());

        public void WriteLine(string? format, params object?[] args)
            => output.WriteLine(format ?? string.Empty, args);
    }
}