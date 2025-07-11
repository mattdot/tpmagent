using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Octokit;
using System.Text.Json;

namespace TpmAgent;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup configuration
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting TPM Agent with Semantic Kernel Process Framework...");

        try
        {
            // Parse GitHub Action inputs
            var issueContent = Environment.GetEnvironmentVariable("INPUT_ISSUE_CONTENT") ?? "";
            var githubToken = Environment.GetEnvironmentVariable("INPUT_GITHUB_TOKEN") ?? "";
            var repository = Environment.GetEnvironmentVariable("INPUT_REPOSITORY") ?? "";
            var issueNumber = Environment.GetEnvironmentVariable("INPUT_ISSUE_NUMBER") ?? "";
            
            // Parse Azure OpenAI configuration (optional)
            var azureApiKey = Environment.GetEnvironmentVariable("INPUT_AZURE_OPENAI_API_KEY") ?? "";
            var azureEndpoint = Environment.GetEnvironmentVariable("INPUT_AZURE_OPENAI_ENDPOINT") ?? "";
            var azureApiVersion = Environment.GetEnvironmentVariable("INPUT_AZURE_OPENAI_API_VERSION") ?? "2023-12-01-preview";
            var azureDeploymentName = Environment.GetEnvironmentVariable("INPUT_AZURE_OPENAI_DEPLOYMENT_NAME") ?? "";

            logger.LogInformation($"Processing issue #{issueNumber} from {repository}");
            
            // Log Azure OpenAI configuration status (without exposing sensitive data)
            var hasAzureConfig = !string.IsNullOrEmpty(azureApiKey) && !string.IsNullOrEmpty(azureEndpoint);
            logger.LogInformation($"Azure OpenAI configuration: {(hasAzureConfig ? "Enabled" : "Disabled")}");
            if (hasAzureConfig)
            {
                logger.LogInformation($"Azure OpenAI endpoint: {azureEndpoint}");
                logger.LogInformation($"Azure OpenAI API version: {azureApiVersion}");
                logger.LogInformation($"Azure OpenAI deployment: {(string.IsNullOrEmpty(azureDeploymentName) ? "Not specified" : azureDeploymentName)}");
            }

            if (string.IsNullOrEmpty(issueContent))
            {
                throw new ArgumentException("Issue content is required");
            }

            if (string.IsNullOrEmpty(githubToken))
            {
                throw new ArgumentException("GitHub token is required");
            }

            if (string.IsNullOrEmpty(repository))
            {
                throw new ArgumentException("Repository is required");
            }

            if (string.IsNullOrEmpty(issueNumber) || !int.TryParse(issueNumber, out int issueNum))
            {
                throw new ArgumentException("Valid issue number is required");
            }

            // Create GitHub client
            var github = new GitHubClient(new ProductHeaderValue("tpm-agent"))
            {
                Credentials = new Credentials(githubToken)
            };

            // Create and run the issue processing agent using Process Framework pattern
            var azureOpenAIConfig = new AzureOpenAIConfig
            {
                ApiKey = azureApiKey,
                Endpoint = azureEndpoint,
                ApiVersion = azureApiVersion,
                DeploymentName = azureDeploymentName
            };
            
            var agent = new IssueProcessingAgent(logger, github, azureOpenAIConfig);
            var result = await agent.ProcessIssueAsync(issueContent, repository, issueNum);
            
            logger.LogInformation($"Issue processing completed: {result}");
            
            // Set GitHub Actions outputs
            await SetGitHubOutput("result", result);
            await SetGitHubOutput("status", "success");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during issue processing");
            
            await SetGitHubOutput("result", $"Error: {ex.Message}");
            await SetGitHubOutput("status", "error");
            
            Environment.Exit(1);
        }
    }

    private static async Task SetGitHubOutput(string name, string value)
    {
        var outputFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
        var outputLine = $"{name}={value}";
        
        if (!string.IsNullOrEmpty(outputFile))
        {
            await File.AppendAllTextAsync(outputFile, outputLine + Environment.NewLine);
        }
        else
        {
            // Fallback for testing
            await File.AppendAllTextAsync("/tmp/github_output.txt", outputLine + Environment.NewLine);
        }
    }
}

public class IssueProcessingAgent
{
    private readonly ILogger _logger;
    private readonly GitHubClient _github;
    private readonly Kernel _kernel;
    private readonly AzureOpenAIConfig _azureConfig;
    private readonly bool _hasAzureOpenAI;

    public IssueProcessingAgent(ILogger logger, GitHubClient github, AzureOpenAIConfig azureConfig)
    {
        _logger = logger;
        _github = github;
        _azureConfig = azureConfig;
        _hasAzureOpenAI = !string.IsNullOrEmpty(azureConfig.ApiKey) && !string.IsNullOrEmpty(azureConfig.Endpoint);
        
        // Create kernel for process-like workflow
        var builder = Kernel.CreateBuilder();
        
        // Configure Azure OpenAI if credentials are provided
        if (_hasAzureOpenAI)
        {
            _logger.LogInformation("Configuring kernel with Azure OpenAI");
            
            // Use deployment name if provided, otherwise use a default model name
            var deploymentName = string.IsNullOrEmpty(azureConfig.DeploymentName) ? "gpt-4" : azureConfig.DeploymentName;
            
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: deploymentName,
                endpoint: azureConfig.Endpoint,
                apiKey: azureConfig.ApiKey,
                apiVersion: azureConfig.ApiVersion
            );
        }
        else
        {
            _logger.LogInformation("No Azure OpenAI configuration provided, using basic analysis");
        }
        
        _kernel = builder.Build();
    }

    public async Task<string> ProcessIssueAsync(string issueContent, string repository, int issueNumber)
    {
        _logger.LogInformation($"Starting issue processing with Semantic Kernel Process Framework pattern");

        try
        {
            // Execute process steps in sequence (simulating Process Framework)
            var processContext = new ProcessContext
            {
                IssueContent = issueContent,
                Repository = repository,
                IssueNumber = issueNumber,
                GitHub = _github,
                Logger = _logger
            };

            // Step 1: Analyze the issue
            var analysisResult = await ExecuteAnalysisStepAsync(processContext);
            processContext.Analysis = analysisResult;

            // Step 2: Generate comment
            var comment = await ExecuteCommentGenerationStepAsync(processContext);
            processContext.GeneratedComment = comment;

            // Step 3: Post comment to GitHub
            await ExecutePostCommentStepAsync(processContext);

            return "Issue processed successfully using Semantic Kernel Process Framework pattern";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in issue processing");
            throw;
        }
    }

    private async Task<IssueAnalysis> ExecuteAnalysisStepAsync(ProcessContext context)
    {
        context.Logger.LogInformation("::group::Analyzing issue content");
        
        try
        {
            var analysis = new IssueAnalysis();
            
            if (_hasAzureOpenAI)
            {
                // Use Azure OpenAI for intelligent analysis
                analysis = await AnalyzeIssueWithAIAsync(context.IssueContent);
                context.Logger.LogInformation("Analysis completed using Azure OpenAI");
            }
            else
            {
                // Fallback to basic keyword analysis
                analysis = AnalyzeIssueBasic(context.IssueContent);
                context.Logger.LogInformation("Analysis completed using basic keyword detection");
            }
            
            context.Logger.LogInformation($"Analysis results - Type: {analysis.Type}, Priority: {analysis.Priority}, Topics: {string.Join(", ", analysis.Topics)}");
            
            return analysis;
        }
        finally
        {
            context.Logger.LogInformation("::endgroup::");
        }
    }

    private async Task<IssueAnalysis> AnalyzeIssueWithAIAsync(string issueContent)
    {
        try
        {
            var prompt = $@"
Analyze the following GitHub issue and provide a JSON response with the following structure:
{{
  ""type"": ""bug"" | ""feature"" | ""question"",
  ""priority"": ""low"" | ""medium"" | ""high"",
  ""topics"": [""list"", ""of"", ""relevant"", ""topics""],
  ""summary"": ""brief summary of the issue""
}}

Issue content:
{issueContent}

Focus on identifying:
1. Whether this is a bug report, feature request, or question
2. The priority level based on urgency indicators
3. Key topics related to TPM, security, authentication, encryption, Docker, containers, etc.
4. A concise summary

Respond only with valid JSON.";

            var response = await _kernel.InvokePromptAsync(prompt);
            var result = response.GetValue<string>();
            
            if (string.IsNullOrEmpty(result))
            {
                _logger.LogWarning("Empty response from Azure OpenAI, falling back to basic analysis");
                return AnalyzeIssueBasic(issueContent);
            }

            // Parse JSON response
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(result);
            
            var analysis = new IssueAnalysis
            {
                Type = jsonResponse.GetProperty("type").GetString() ?? "question",
                Priority = jsonResponse.GetProperty("priority").GetString() ?? "medium",
                Summary = jsonResponse.GetProperty("summary").GetString() ?? "AI analysis completed"
            };
            
            // Parse topics array
            if (jsonResponse.TryGetProperty("topics", out var topicsElement) && topicsElement.ValueKind == JsonValueKind.Array)
            {
                analysis.Topics = topicsElement.EnumerateArray()
                    .Select(t => t.GetString() ?? "")
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
            }
            
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during AI analysis, falling back to basic analysis");
            return AnalyzeIssueBasic(issueContent);
        }
    }

    private IssueAnalysis AnalyzeIssueBasic(string issueContent)
    {
        // Original basic analysis logic
        var analysis = new IssueAnalysis();
        var content = issueContent.ToLower();
        
        // Determine issue type
        if (content.Contains("bug") || content.Contains("error") || content.Contains("issue"))
        {
            analysis.Type = "bug";
        }
        else if (content.Contains("feature") || content.Contains("enhancement") || content.Contains("request"))
        {
            analysis.Type = "feature";
        }
        else
        {
            analysis.Type = "question";
        }

        // Determine priority
        if (content.Contains("urgent") || content.Contains("critical") || content.Contains("high"))
        {
            analysis.Priority = "high";
        }
        else if (content.Contains("low") || content.Contains("minor"))
        {
            analysis.Priority = "low";
        }
        else
        {
            analysis.Priority = "medium";
        }

        // Extract key topics
        var topics = new List<string>();
        if (content.Contains("tpm")) topics.Add("TPM");
        if (content.Contains("security")) topics.Add("Security");
        if (content.Contains("authentication")) topics.Add("Authentication");
        if (content.Contains("encryption")) topics.Add("Encryption");
        if (content.Contains("docker")) topics.Add("Docker");
        if (content.Contains("container")) topics.Add("Container");
        if (content.Contains("openai")) topics.Add("OpenAI");
        if (content.Contains("azure")) topics.Add("Azure");
        
        analysis.Topics = topics;
        analysis.Summary = issueContent.Length > 100 
            ? issueContent.Substring(0, 100) + "..."
            : issueContent;
            
        return analysis;
    }

    private async Task<string> ExecuteCommentGenerationStepAsync(ProcessContext context)
    {
        context.Logger.LogInformation("::group::Generating response comment");
        
        try
        {
            // Generate a response comment based on the analysis
            var comment = await Task.FromResult(GenerateContextualComment(context.Analysis, context.IssueContent));
            
            context.Logger.LogInformation($"Generated comment with {comment.Length} characters");
            
            return comment;
        }
        finally
        {
            context.Logger.LogInformation("::endgroup::");
        }
    }

    private async Task ExecutePostCommentStepAsync(ProcessContext context)
    {
        context.Logger.LogInformation("::group::Posting comment to GitHub");
        
        try
        {
            var repoParts = context.Repository.Split('/');
            if (repoParts.Length != 2)
            {
                throw new ArgumentException("Repository must be in format owner/repo");
            }
            
            var owner = repoParts[0];
            var repo = repoParts[1];
            
            // Post the comment to GitHub
            await context.GitHub.Issue.Comment.Create(owner, repo, context.IssueNumber, context.GeneratedComment);
            
            context.Logger.LogInformation($"Comment posted successfully to {context.Repository}#{context.IssueNumber}");
        }
        finally
        {
            context.Logger.LogInformation("::endgroup::");
        }
    }

    private string GenerateContextualComment(IssueAnalysis analysis, string issueContent)
    {
        var analysisMethod = _hasAzureOpenAI ? "Azure OpenAI" : "keyword analysis";
        var comment = $"Thank you for this issue! I've analyzed it using {analysisMethod} with the Semantic Kernel Process Framework.\n\n";
        
        comment += "## Analysis Results\n\n";
        comment += $"- **Type**: {analysis.Type}\n";
        comment += $"- **Priority**: {analysis.Priority}\n";
        
        if (analysis.Topics.Any())
        {
            comment += $"- **Topics**: {string.Join(", ", analysis.Topics)}\n";
        }
        
        if (!string.IsNullOrEmpty(analysis.Summary) && analysis.Summary != issueContent)
        {
            comment += $"- **Summary**: {analysis.Summary}\n";
        }
        
        comment += "\n## Next Steps\n\n";
        
        switch (analysis.Type)
        {
            case "bug":
                comment += "This appears to be a bug report. The development team will:\n";
                comment += "1. Review the issue details\n";
                comment += "2. Reproduce the issue if possible\n";
                comment += "3. Investigate the root cause\n";
                comment += "4. Provide a fix or workaround\n";
                break;
                
            case "feature":
                comment += "This appears to be a feature request. The team will:\n";
                comment += "1. Evaluate the request against project goals\n";
                comment += "2. Assess implementation complexity\n";
                comment += "3. Consider adding it to the roadmap\n";
                comment += "4. Provide feedback on feasibility\n";
                break;
                
            default:
                comment += "This appears to be a question or general issue. The team will:\n";
                comment += "1. Review the details provided\n";
                comment += "2. Provide clarification or guidance\n";
                comment += "3. Update documentation if needed\n";
                break;
        }
        
        comment += "\n## Additional Information\n\n";
        
        if (analysis.Topics.Contains("TPM"))
        {
            comment += "Since this relates to TPM functionality, please ensure you have:\n";
            comment += "- TPM hardware or software available\n";
            comment += "- Proper permissions for TPM operations\n";
            comment += "- Latest version of the TPM Agent\n\n";
        }
        
        if (analysis.Topics.Contains("Docker"))
        {
            comment += "For Docker-related issues, please provide:\n";
            comment += "- Docker version information\n";
            comment += "- Container logs if applicable\n";
            comment += "- Environment details\n\n";
        }
        
        if (analysis.Topics.Contains("OpenAI") || analysis.Topics.Contains("Azure"))
        {
            comment += "For Azure OpenAI configuration issues, please check:\n";
            comment += "- API key validity and permissions\n";
            comment += "- Endpoint URL format (should include https://)\n";
            comment += "- Deployment name matches your Azure OpenAI resource\n";
            comment += "- API version compatibility\n\n";
        }
        
        comment += "---\n";
        comment += $"*This response was generated automatically using the Semantic Kernel Process Framework with {analysisMethod}.*";
        
        return comment;
    }
}

public class ProcessContext
{
    public string IssueContent { get; set; } = "";
    public string Repository { get; set; } = "";
    public int IssueNumber { get; set; }
    public GitHubClient GitHub { get; set; } = null!;
    public ILogger Logger { get; set; } = null!;
    public IssueAnalysis Analysis { get; set; } = new();
    public string GeneratedComment { get; set; } = "";
}

public class IssueAnalysis
{
    public string Type { get; set; } = "";
    public string Priority { get; set; } = "";
    public List<string> Topics { get; set; } = new();
    public string Summary { get; set; } = "";
}

public class AzureOpenAIConfig
{
    public string ApiKey { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string ApiVersion { get; set; } = "";
    public string DeploymentName { get; set; } = "";
}