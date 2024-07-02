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