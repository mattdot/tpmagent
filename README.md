# TPM Agent

A Docker container-based GitHub Action for processing GitHub issues using Semantic Kernel Process Framework.

## Architecture

This action runs inside a Docker container and uses a C# application built with Semantic Kernel 1.60 to analyze GitHub issues and provide intelligent responses. The container-based approach provides:

- **Isolation**: Each action run gets a fresh, isolated environment
- **Consistency**: Same runtime environment across different GitHub runners
- **Semantic Kernel Integration**: Advanced AI-powered issue analysis and response generation
- **Process Framework**: Uses Semantic Kernel Process Framework pattern for multi-step workflows
- **GitHub Integration**: Direct integration with GitHub API for issue commenting

## Components

- **Dockerfile**: Defines the container runtime environment with .NET 8.0 and required dependencies
- **C# Agent**: Semantic Kernel-based application that processes GitHub issues using Process Framework patterns
- **Entrypoint Script**: Orchestrates the container execution and GitHub Actions integration
- **GitHub API Integration**: Uses Octokit for GitHub API interactions

## Usage

### Basic Usage (without Azure OpenAI)

```yaml
- name: Process GitHub Issue
  uses: mattdot/tpmagent@v1
  with:
    issue_content: ${{ github.event.issue.body }}
    github_token: ${{ secrets.GITHUB_TOKEN }}
    repository: ${{ github.repository }}
    issue_number: ${{ github.event.issue.number }}
```

### Advanced Usage with Azure OpenAI

```yaml
- name: Process GitHub Issue with Azure OpenAI
  uses: mattdot/tpmagent@v1
  with:
    issue_content: ${{ github.event.issue.body }}
    github_token: ${{ secrets.GITHUB_TOKEN }}
    repository: ${{ github.repository }}
    issue_number: ${{ github.event.issue.number }}
    azure_openai_api_key: ${{ secrets.AZURE_OPENAI_API_KEY }}
    azure_openai_endpoint: ${{ secrets.AZURE_OPENAI_ENDPOINT }}
    azure_openai_deployment_name: ${{ secrets.AZURE_OPENAI_DEPLOYMENT_NAME }}
    azure_openai_api_version: '2023-12-01-preview' # Optional, defaults to this version
```

### Advanced Usage with Issue Events

```yaml
name: Process Issues
on:
  issues:
    types: [opened, edited]

jobs:
  process-issue:
    runs-on: ubuntu-latest
    steps:
      - name: Process Issue with TPM Agent
        uses: mattdot/tpmagent@v1
        with:
          issue_content: ${{ github.event.issue.body }}
          github_token: ${{ secrets.GITHUB_TOKEN }}
          repository: ${{ github.repository }}
          issue_number: ${{ github.event.issue.number }}
        id: issue-processor

      - name: Check Processing Results
        run: |
          echo "Processing Status: ${{ steps.issue-processor.outputs.status }}"
          echo "Processing Result: ${{ steps.issue-processor.outputs.result }}"
```

## Inputs

| Input | Description | Required | Default |
|-------|-------------|----------|---------|
| `issue_content` | The content of the GitHub issue to process | Yes | - |
| `github_token` | GitHub token for API access | Yes | - |
| `repository` | Repository in the format owner/repo | Yes | - |
| `issue_number` | Issue number to comment on | Yes | - |
| `azure_openai_api_key` | Azure OpenAI API key for AI-powered analysis | No | - |
| `azure_openai_endpoint` | Azure OpenAI endpoint URL | No | - |
| `azure_openai_deployment_name` | Azure OpenAI deployment name for the model | No | - |
| `azure_openai_api_version` | Azure OpenAI API version | No | `2023-12-01-preview` |

## Outputs

| Output | Description |
|--------|-------------|
| `result` | Result of the issue processing |
| `status` | Status of the operation (`success`, `error`) |

## Process Framework Workflow

The action implements a Semantic Kernel Process Framework pattern with the following steps:

1. **Issue Analysis**: Analyzes the issue content to determine:
   - Issue type (bug, feature, question)
   - Priority level (high, medium, low)
   - Related topics (TPM, Security, Docker, etc.)

2. **Comment Generation**: Creates an intelligent response based on:
   - Analysis results
   - Contextual recommendations
   - Next steps and guidance

3. **GitHub Integration**: Posts the generated comment to the issue using:
   - GitHub API via Octokit
   - Proper authentication and error handling
   - Structured formatting

## Container Architecture

The action is built as a multi-stage Docker container:

1. **Build Stage**: Uses .NET 8.0 SDK to build the C# Semantic Kernel application
2. **Runtime Stage**: Uses .NET 8.0 runtime with required dependencies for execution
3. **Entrypoint**: Bash script that orchestrates the C# agent execution

## Example Workflow

```yaml
name: Issue Processing
on:
  issues:
    types: [opened, edited]

jobs:
  process-issue:
    runs-on: ubuntu-latest
    steps:
      - name: Process Issue with Azure OpenAI
        uses: mattdot/tpmagent@v1
        with:
          issue_content: ${{ github.event.issue.body }}
          github_token: ${{ secrets.GITHUB_TOKEN }}
          repository: ${{ github.repository }}
          issue_number: ${{ github.event.issue.number }}
          azure_openai_api_key: ${{ secrets.AZURE_OPENAI_API_KEY }}
          azure_openai_endpoint: ${{ secrets.AZURE_OPENAI_ENDPOINT }}
          azure_openai_deployment_name: ${{ secrets.AZURE_OPENAI_DEPLOYMENT_NAME }}
        id: issue-processor
      
      - name: Display Processing Results
        run: |
          echo "Processing Status: ${{ steps.issue-processor.outputs.status }}"
          echo "Processing Result: ${{ steps.issue-processor.outputs.result }}"
```

### Fallback Configuration

If Azure OpenAI is not configured or fails, the agent automatically falls back to keyword-based analysis:

```yaml
name: Issue Processing (Basic Mode)
on:
  issues:
    types: [opened, edited]

jobs:
  process-issue:
    runs-on: ubuntu-latest
    steps:
      - name: Process Issue (Basic Analysis)
        uses: mattdot/tpmagent@v1
        with:
          issue_content: ${{ github.event.issue.body }}
          github_token: ${{ secrets.GITHUB_TOKEN }}
          repository: ${{ github.repository }}
          issue_number: ${{ github.event.issue.number }}
        id: issue-processor
      
      - name: Display Processing Results
        run: |
          echo "Processing Status: ${{ steps.issue-processor.outputs.status }}"
          echo "Processing Result: ${{ steps.issue-processor.outputs.result }}"
```

## Features

### Intelligent Issue Analysis

The agent analyzes GitHub issues to determine:
- **Issue Type**: Automatically categorizes as bug, feature request, or question
- **Priority Level**: Assigns high, medium, or low priority based on keywords
- **Topic Detection**: Identifies relevant topics (TPM, Security, Docker, etc.)

### Dual Analysis Modes

**Basic Analysis Mode (Default)**:
- Uses keyword-based analysis for issue classification
- No external dependencies required
- Provides consistent, fast analysis

**Azure OpenAI Mode (Optional)**:
- Leverages Azure OpenAI for intelligent issue understanding
- Provides more nuanced analysis and topic detection
- Generates contextual summaries and insights
- Requires Azure OpenAI configuration

### Azure OpenAI Configuration

To enable AI-powered analysis, configure the following secrets in your repository:

1. **AZURE_OPENAI_API_KEY**: Your Azure OpenAI API key
2. **AZURE_OPENAI_ENDPOINT**: Your Azure OpenAI endpoint URL (e.g., `https://your-resource.openai.azure.com`)
3. **AZURE_OPENAI_DEPLOYMENT_NAME**: The name of your deployed model (e.g., `gpt-4`, `gpt-35-turbo`)

**Security Note**: Always use GitHub repository secrets for sensitive configuration. Never hardcode API keys or endpoints in your workflow files.

#### Setting up Azure OpenAI

1. Create an Azure OpenAI resource in the Azure portal
2. Deploy a model (GPT-4 or GPT-3.5-Turbo recommended)
3. Copy the endpoint URL and API key
4. Add these values as secrets in your GitHub repository settings
5. Reference them in your workflow using `${{ secrets.SECRET_NAME }}`

### Contextual Response Generation

Based on the analysis, the agent generates:
- **Structured Comments**: Well-formatted responses with sections
- **Actionable Guidance**: Specific next steps based on issue type
- **Context-Aware Recommendations**: Tailored advice for different topics

### GitHub Integration

- **Automatic Commenting**: Posts responses directly to GitHub issues
- **API Integration**: Uses Octokit for robust GitHub API interactions
- **Error Handling**: Comprehensive error management with detailed logging

## Development

### Local Development

To build and test the Docker container locally:

```bash
# Build the container
docker build -t tpmagent .

# Test the container
# Test the container with sample issue content
docker run \
  -e INPUT_ISSUE_CONTENT="This is a bug report about TPM functionality" \
  -e INPUT_GITHUB_TOKEN="your-token" \
  -e INPUT_REPOSITORY="owner/repo" \
  -e INPUT_ISSUE_NUMBER="1" \
  tpmagent
```

### C# Application Structure

The core application is built with:
- **Semantic Kernel 1.60**: For AI-powered issue analysis using Process Framework patterns
- **.NET 8.0**: Modern, performant runtime
- **Octokit**: GitHub API integration for issue commenting
- **Dependency Injection**: Clean architecture with logging and configuration
- **Environment Variables**: GitHub Actions input/output integration

### Process Framework Implementation

The application implements a process-like workflow:

1. **ProcessContext**: Maintains state throughout the workflow
2. **Analysis Step**: Analyzes issue content and categorizes it
3. **Generation Step**: Creates contextual responses based on analysis
4. **Integration Step**: Posts comments to GitHub using the API

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.