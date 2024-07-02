using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace SemanticKernelSamples;

public class BaseTest
{
    private readonly ServiceProvider _serviceProvider;
    protected IXunitOutput Output { get; }

    protected BaseTest(ITestOutputHelper output)
    {
        this.Output = new XunitOutput(output);

        var configRoot = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", true)
            //.AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        TestConfiguration.Initialize(configRoot);
    }
}