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

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SemanticKernelSamples;

public class LoggingTests : BaseTest
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ILogger<LoggingTests> _logger;
    private readonly Kernel _kernel;

    public LoggingTests(ITestOutputHelper output) : base(output)
    {
        var services = new ServiceCollection();

        services.AddOpenTelemetryLogging(output);

        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.Services.Add(services);

        this._kernel = builder.AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey
                ).Build();

        _logger = _kernel.Services.GetRequiredService<ILogger<LoggingTests>>();
    }

    /// <summary>
    /// Output:
    /// Information: Starting logging tests...
    /// Trace: Extracting blocks from template: What color is the sky?
    /// Information: Function InvokePromptAsync_27be001d322c43dfb00fd38a6bd0ea0a invoking.
    /// Trace: Function arguments: {}
    /// Trace: Rendered prompt: What color is the sky?
    /// Trace: ChatHistory: [{"Role":{"Label":"user"},"Items":[{"$type":"TextContent","Text":"What color is the sky?"}]}], Settings: { "temperature":1,"top_p":1,"presence_penalty":0,"frequency_penalty":0,"max_tokens":null,"stop_sequences":null,"results_per_prompt":1,"seed":null,"response_format":null,"chat_system_prompt":null,"token_selection_biases":null,"ToolCallBehavior":null,"User":null,"logprobs":null,"top_logprobs":null,"service_id":null,"model_id":null}
    /// Information: Prompt tokens: 13.Completion tokens: 47.Total tokens: 60.
    /// Information: Prompt tokens: 13.Completion tokens: 47.
    /// Information: Function InvokePromptAsync_27be001d322c43dfb00fd38a6bd0ea0a succeeded.
    /// Trace: Function result: During a clear day, the sky is blue. However, it can change to various colors such as grey during cloudy conditions, or red, pink, orange, and purple during sunrise or sunset. At night, the sky is black.
    /// Information: Function completed. Duration: 2.5831082s
    /// Information: result: During a clear day, the sky is blue. However, it can change to various colors such as grey during cloudy conditions, or red, pink, orange, and purple during sunrise or sunset. At night, the sky is black.
    /// Information: Logging tests complete.
    /// </summary>
    [Fact]
    public async Task RunAsync()
    {
        ActivitySource s_activitySource = new("Telemetry.Example");
        using var activity = s_activitySource.StartActivity("Start");

        using (activity)
        {
            Console.WriteLine($"Operation/Trace ID: {Activity.Current?.TraceId}");
            _logger.LogInformation("Starting logging tests...");

            var result = await _kernel.InvokePromptAsync("What color is the sky?");
            _logger.LogInformation($"result: {result}");
            _logger.LogInformation("Logging tests complete.");
        }

        var tracerProvider = _kernel.Services.GetRequiredService<TracerProvider>();
        var meterProvider = _kernel.Services.GetRequiredService<MeterProvider>();
        var loggerProvider = _kernel.Services.GetRequiredService<LoggerProvider>();
        tracerProvider.ForceFlush();
        meterProvider.ForceFlush();
        loggerProvider.ForceFlush();
    }
}

public class ActivityEnrichingProcessor : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        activity.SetTag("user_id", "test_user_id_1");
        activity.SetTag("ActivityKind", activity.Kind);
    }
}

public static class LoggingExtensions
{
    public static IServiceCollection AddOpenTelemetryLogging(this IServiceCollection services, ITestOutputHelper output)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService("semantic-kernel-sample", serviceVersion: "1.0.0-demo");

        var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddSource("Microsoft.SemanticKernel*")
            .AddSource("SemanticKernelSamples")
            .AddProcessor(new ActivityEnrichingProcessor())
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
            builder.AddProvider(new XunitLoggerProvider(output, LogLevel.Trace));

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