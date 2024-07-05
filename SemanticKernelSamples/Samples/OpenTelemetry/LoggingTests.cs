using System.Diagnostics;

using Azure.Monitor.OpenTelemetry.Exporter;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using SemanticKernelSamples.Logging;

using Xunit.Abstractions;

namespace SemanticKernelSamples.Samples.OpenTelemetry;

public class LoggingTests : BaseTest
{
    private readonly ILogger<LoggingTests> _logger;
    private readonly Kernel _kernel;

    public LoggingTests(ITestOutputHelper output) : base(output)
    {
        var services = new ServiceCollection();

        ConfigureServices(services, output);

        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.Services.Add(services);

        _kernel = builder.AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey
                ).Build();

        _logger = _kernel.Services.GetRequiredService<ILogger<LoggingTests>>();
    }

    [Fact]
    public async Task RunAsync()
    {
        ActivitySource s_activitySource = new("SemanticKernelSamples");

        using var activity = s_activitySource.StartActivity("Start");

        _logger.LogInformation($"Starting logging tests, Operation/Trace ID: {Activity.Current?.TraceId}");

        var result = await _kernel.InvokePromptAsync("What color is the sky?");
        _logger.LogInformation($"result: {result}");
        _logger.LogInformation("Logging tests complete.");

        var tracerProvider = _kernel.Services.GetRequiredService<TracerProvider>();
        var meterProvider = _kernel.Services.GetRequiredService<MeterProvider>();
        var loggerProvider = _kernel.Services.GetRequiredService<LoggerProvider>();
        tracerProvider.ForceFlush();
        meterProvider.ForceFlush();
        loggerProvider.ForceFlush();

        //Output: 
        //[Information - SemanticKernelSamples.Samples.OpenTelemetry.LoggingTests]: Starting logging tests, Operation/Trace ID: 01436b697db5273080bfcaa9c007cffa
        //[Trace - Microsoft.SemanticKernel.KernelPromptTemplate]: Extracting blocks from template: What color is the sky?
        //[Information - InvokePromptAsync_1bc2ed172b6c44438544ce7c6cf18d5d]: Function InvokePromptAsync_1bc2ed172b6c44438544ce7c6cf18d5d invoking.
        //[Trace - InvokePromptAsync_1bc2ed172b6c44438544ce7c6cf18d5d]: Function arguments: {}
        //[Trace - Microsoft.SemanticKernel.KernelFunctionFactory]: Rendered prompt: What color is the sky?
        //[Trace - Microsoft.SemanticKernel.Connectors.OpenAI.AzureOpenAIChatCompletionService]: ChatHistory: [{"Role":{"Label":"user"},"Items":[{"$type":"TextContent","Text":"What color is the sky?"}]}], Settings: { "temperature":1,"top_p":1,"presence_penalty":0,"frequency_penalty":0,"max_tokens":null,"stop_sequences":null,"results_per_prompt":1,"seed":null,"response_format":null,"chat_system_prompt":null,"token_selection_biases":null,"ToolCallBehavior":null,"User":null,"logprobs":null,"top_logprobs":null,"service_id":null,"model_id":null}
        //[Information - Microsoft.SemanticKernel.Connectors.OpenAI.AzureOpenAIChatCompletionService]: Prompt tokens: 13.Completion tokens: 56.Total tokens: 69.
        //[Information - Microsoft.SemanticKernel.KernelFunctionFactory]: Prompt tokens: 13.Completion tokens: 56.
        //[Information - InvokePromptAsync_1bc2ed172b6c44438544ce7c6cf18d5d]: Function InvokePromptAsync_1bc2ed172b6c44438544ce7c6cf18d5d succeeded.
        //[Trace - InvokePromptAsync_1bc2ed172b6c44438544ce7c6cf18d5d]: Function result: The sky is usually blue during a clear day. However, it can change color depending on the time of day and weather conditions. For example, it can appear red, orange, pink, or purple during sunrise or sunset, and grey or even green during certain types of storms.
        //[Information - InvokePromptAsync_1bc2ed172b6c44438544ce7c6cf18d5d]: Function completed. Duration: 2.8751071s
        //[Information - SemanticKernelSamples.Samples.OpenTelemetry.LoggingTests]: result: The sky is usually blue during a clear day. However, it can change color depending on the time of day and weather conditions. For example, it can appear red, orange, pink, or purple during sunrise or sunset, and grey or even green during certain types of storms.
        //[Information - SemanticKernelSamples.Samples.OpenTelemetry.LoggingTests]: Logging tests complete.
    }

    private static IServiceCollection ConfigureServices(IServiceCollection services, ITestOutputHelper output)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService("semantic-kernel-sample", serviceVersion: "1.0.0-demo");

        var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddSource("Microsoft.SemanticKernel*")
            .AddSource("SemanticKernelSamples")
            .AddAzureMonitorTraceExporter(options =>
            {
                options.ConnectionString = TestConfiguration.ApplicationInsights.ConnectionString;
                options.SamplingRatio = 1.0f;
            })
            .Build();

        var meterProvider = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddMeter("Microsoft.SemanticKernel*")
            .AddAzureMonitorMetricExporter(options => options.ConnectionString = TestConfiguration.ApplicationInsights.ConnectionString)
            .Build();

        services.AddSingleton(tracerProvider);
        services.AddSingleton(meterProvider);

        services.AddLogging(builder =>
        {
            builder.AddProvider(new XunitLoggerProvider(output, LogLevel.Trace));    // used to capture logs from the test output

            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.AddAzureMonitorLogExporter(options => options.ConnectionString = TestConfiguration.ApplicationInsights.ConnectionString);
            });
            builder.SetMinimumLevel(LogLevel.Trace);
        });

        return services;
    }
}