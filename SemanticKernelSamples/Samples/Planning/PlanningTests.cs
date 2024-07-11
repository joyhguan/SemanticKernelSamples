using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Planning.Handlebars;

using Xunit.Abstractions;

namespace SemanticKernelSamples.Samples.Planning
{
    public class PlanningTests : BaseTest
    {
        private readonly Kernel _kernel;

        public PlanningTests(ITestOutputHelper output) : base(output)
        {
            var services = new ServiceCollection();

            var builder = Kernel.CreateBuilder();

            builder.Services.Add(services);

            builder.AddAzureOpenAIChatCompletion(
                    deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                    endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                    apiKey: TestConfiguration.AzureOpenAI.ApiKey
                    );

            _kernel = builder.Build();
        }

        [Fact]
        public async Task RunAsync()
        {
            _kernel.ImportPluginFromType<DocumentPlugins>();
            _kernel.ImportPluginFromType<UserPlugins>();

            var instruction = @"First, if the document details can answer the user's request, use it to retrieve the information.
If the document details doesn't have enough context or can not answer the user's request, check if the information is available in custom fields.
Custom fields are additional data attributes associated with documents or entities.
if the information from the detail and custom fields is still insufficient, retrieve the content from the document.

<user_request> $user_request </user_request>
";

            var user_request = "Whis the background of Privacy Agreement?";

#pragma warning disable SKEXP0060
            var plannerOptions = new HandlebarsPlannerOptions()
            {
                // When using OpenAI models, we recommend using low values for temperature and top_p to minimize planner hallucinations.
                ExecutionSettings = new OpenAIPromptExecutionSettings()
                {
                    Temperature = 0.0,
                    TopP = 0.1,
                },
                AllowLoops = true
            };
            var planner = new HandlebarsPlanner(plannerOptions);
            var arguments = new KernelArguments { ["user_request"] = user_request };
            var plan = await planner.CreatePlanAsync(_kernel, instruction, arguments);

            var result = await plan.InvokeAsync(_kernel, arguments);

            Output.WriteLine("Plan:\n");
            Output.WriteLine(plan);
            /*
Original plan:

{{!-- Step 0: Extract key values --}}
{{set "user_request" @root.user_request}}

{{!-- Step 1: Search for document names that match the user request --}}
{{set "documentIds" (DocumentPlugins-SearchDocumentNames searchQuery=user_request)}}

{{!-- Step 2: Loop through each document ID --}}
{{#each documentIds as |documentId|}}
  {{!-- Step 3: Get document details --}}
  {{set "documentDetails" (DocumentPlugins-GetDocumentDetails id=documentId)}}

  {{!-- Step 4: Check if document details answer the user request --}}
  {{#if (equals documentDetails.user_request true)}}
    {{json documentDetails}}
  {{else}}
    {{!-- Step 5: Get custom field names --}}
    {{set "customFieldNames" (DocumentPlugins-GetCustomFieldNames documentId=documentId)}}

    {{!-- Step 6: Loop through each custom field name --}}
    {{#each customFieldNames as |fieldName|}}
      {{!-- Step 7: Get custom field values --}}
      {{set "customFieldValues" (DocumentPlugins-GetCustomFieldValues documentId=documentId fieldName=fieldName)}}

      {{!-- Step 8: Check if custom field values answer the user request --}}
      {{#if (equals customFieldValues.user_request true)}}
        {{json customFieldValues}}
      {{else}}
        {{!-- Step 9: Get document contents --}}
        {{set "documentContents" (DocumentPlugins-GetDocumentContents documentId=documentId)}}

        {{!-- Step 10: Check if document contents answer the user request --}}
        {{#if (equals documentContents.user_request true)}}
          {{json documentContents}}
        {{/if}}
      {{/if}}
    {{/each}}
  {{/if}}
{{/each}}
*/

            //Output.WriteLine($"Result: {result}\n");
            //Output.WriteLine("\n======== CreatePlan Prompt ========");
            //Output.WriteLine(originalPlan.Prompt ?? "No Prompts");
        }
    }
}