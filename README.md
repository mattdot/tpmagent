# TPM Agent

A Docker container-based GitHub Action for TPM (Trusted Platform Module) related operations, powered by Semantic Kernel.

## Architecture

This action runs inside a Docker container and uses a C# application built with Semantic Kernel 1.60 to perform TPM-related operations. The container-based approach provides:

- **Isolation**: Each action run gets a fresh, isolated environment
- **Consistency**: Same runtime environment across different GitHub runners
- **Semantic Kernel Integration**: Advanced AI-powered TPM operations and decision-making
- **Comprehensive Tooling**: Built-in TPM tools and utilities

## Components

- **Dockerfile**: Defines the container runtime environment with .NET 8.0 and TPM tools
- **C# Agent**: Semantic Kernel-based application that performs TPM operations
- **Entrypoint Script**: Orchestrates the container execution and GitHub Actions integration

## Usage

### Basic Usage

```yaml
- name: Run TPM Agent
  uses: mattdot/tpmagent@v1
  with:
    operation: 'info'
```

### Advanced Usage

```yaml
- name: Check TPM Status
  uses: mattdot/tpmagent@v1
  with:
    operation: 'check'
    target: 'hardware'
    verbose: 'true'
  id: tpm-check

- name: Use TPM Results
  run: |
    echo "TPM Status: ${{ steps.tpm-check.outputs.status }}"
    echo "TPM Result: ${{ steps.tpm-check.outputs.result }}"
```

## Inputs

| Input | Description | Required | Default |
|-------|-------------|----------|---------|
| `operation` | The operation to perform (`info`, `check`, `validate`) | Yes | `info` |
| `target` | Target for the operation | No | `''` |
| `verbose` | Enable verbose output | No | `false` |

## Outputs

| Output | Description |
|--------|-------------|
| `result` | Result of the operation |
| `status` | Status of the operation (`success`, `error`) |

## Operations

- **info**: Get comprehensive TPM information using Semantic Kernel analysis
- **check**: Intelligent TPM status checking with AI-powered insights
- **validate**: Advanced TPM configuration validation with recommendations

## Container Architecture

The action is built as a multi-stage Docker container:

1. **Build Stage**: Uses .NET 8.0 SDK to build the C# Semantic Kernel application
2. **Runtime Stage**: Uses .NET 8.0 runtime with TPM tools for execution
3. **Entrypoint**: Bash script that orchestrates the C# agent execution

## Example Workflow

```yaml
name: TPM Operations
on: [push, pull_request]

jobs:
  tpm-operations:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Get TPM Info
        uses: mattdot/tpmagent@v1
        with:
          operation: 'info'
          verbose: 'true'
        id: tpm-info
      
      - name: Check TPM
        uses: mattdot/tpmagent@v1
        with:
          operation: 'check'
          target: 'system'
        id: tpm-check
      
      - name: Display Results
        run: |
          echo "Info Result: ${{ steps.tpm-info.outputs.result }}"
          echo "Check Result: ${{ steps.tpm-check.outputs.result }}"
```

## Development

### Local Development

To build and test the Docker container locally:

```bash
# Build the container
docker build -t tpmagent .

# Test the container
docker run -e INPUT_OPERATION=info -e INPUT_VERBOSE=true tpmagent
```

### C# Application Structure

The core application is built with:
- **Semantic Kernel 1.60**: For AI-powered TPM operations
- **.NET 8.0**: Modern, performant runtime
- **Dependency Injection**: Clean architecture with logging and configuration
- **Environment Variables**: GitHub Actions input/output integration

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.