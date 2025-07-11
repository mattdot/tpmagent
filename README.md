# TPM Agent

A reusable GitHub Action for TPM (Trusted Platform Module) related operations.

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

- **info**: Get TPM information
- **check**: Check TPM status
- **validate**: Validate TPM configuration

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

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.