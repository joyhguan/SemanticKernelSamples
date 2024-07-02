using Microsoft.SemanticKernel;

using Xunit.Abstractions;

namespace SemanticKernelSamples;

public class FirstTests : BaseTest
{
    private readonly Kernel _kernel;

    public FirstTests(ITestOutputHelper output) : base(output)
    {
        this._kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey
                ).Build();
    }

    [Fact]
    public async Task Started()
    {
        Output.WriteLine(await _kernel.InvokePromptAsync("What color is the sky?"));
        Output.WriteLine();
    }
}