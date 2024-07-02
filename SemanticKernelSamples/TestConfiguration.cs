using System.Runtime.CompilerServices;

using Microsoft.Extensions.Configuration;

namespace SemanticKernelSamples;

internal class TestConfiguration
{
    private readonly IConfigurationRoot _configRoot;
    private static TestConfiguration? s_instance;
    private static readonly object _lock = new object();

    private TestConfiguration(IConfigurationRoot configRoot)
    {
        _configRoot = configRoot;
    }

    public static void Initialize(IConfigurationRoot configRoot)
    {
        if (s_instance == null)
        {
            lock (_lock)
            {
                if (s_instance == null)
                {
                    s_instance = new TestConfiguration(configRoot);
                }
            }
        }
    }

    public static AzureOpenAIConfig AzureOpenAI => LoadSection<AzureOpenAIConfig>();

    public static ApplicationInsightsConfig ApplicationInsights => LoadSection<ApplicationInsightsConfig>();

    private static T LoadSection<T>([CallerMemberName] string? caller = null)
    {
        if (string.IsNullOrEmpty(caller))
        {
            throw new ArgumentNullException(nameof(caller), "Caller member name cannot be null or empty.");
        }

        if (s_instance == null)
        {
            throw new InvalidOperationException("TestConfiguration has not been initialized.");
        }

        T section = s_instance._configRoot.GetSection(caller).Get<T>()
            ?? throw new InvalidOperationException($"Configuration section '{caller}' not found or cannot be loaded into type '{typeof(T).FullName}'.");

        return section;
    }
}

internal class AzureOpenAIConfig
{
    public string ServiceId { get; set; }
    public string DeploymentName { get; set; }
    public string ModelId { get; set; }
    public string Endpoint { get; set; }
    public string ApiKey { get; set; }
}

internal class ApplicationInsightsConfig
{
    public string ConnectionString { get; set; }
}