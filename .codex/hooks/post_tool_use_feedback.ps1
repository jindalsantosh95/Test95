$ErrorActionPreference = 'Stop'

function Write-Feedback([string]$message, [string]$context) {
    $payload = @{
        systemMessage = $message
        hookSpecificOutput = @{
            hookEventName = 'PostToolUse'
            additionalContext = $context
        }
    } | ConvertTo-Json -Depth 8 -Compress

    Write-Output $payload
}

function Record-Verification([string]$Command, [bool]$Failed) {
    $repoRootRaw = (& git rev-parse --show-toplevel 2>$null).Trim()
    if ([string]::IsNullOrWhiteSpace($repoRootRaw)) { return }
    $lifecycle = Join-Path $repoRootRaw 'scripts/update-agent-memory-state.ps1'
    $result = if ($Failed) { 'fail' } else { 'pass' }
    try { & $lifecycle -Action RecordVerification -VerificationCommand $Command -VerificationResult $result -Actor 'codex-post-tool' *> $null } catch { }
}

try {
    $raw = [Console]::In.ReadToEnd()
    if ([string]::IsNullOrWhiteSpace($raw)) {
        $raw = ($input | Out-String)
    }

    if ([string]::IsNullOrWhiteSpace($raw)) {
        exit 0
    }

    $hookPayload = $raw | ConvertFrom-Json
    if ([string]$hookPayload.hook_event_name -ne 'PostToolUse') {
        exit 0
    }

    $toolName = [string]$hookPayload.tool_name

    if ($toolName -eq 'Bash') {
        $command = [string]$hookPayload.tool_input.command
        if ([string]::IsNullOrWhiteSpace($command)) {
            exit 0
        }

        if ($command -notmatch '(?im)(^|[;&|]\s*)(?:&\s*)?(?:\.[/\\])?(?:dotnet\s+(build|test)\b|scripts[/\\]verify(?:-staged)?\.ps1\b)') {
            exit 0
        }

        $responseText = ($hookPayload.tool_response | ConvertTo-Json -Compress -Depth 12)
        $failed = $responseText -match '(?i)"exit_?code"\s*:\s*[1-9]\d*' -or $responseText -match '(?i)"(?:isError|success)"\s*:\s*(?:true|false)' -and $responseText -match '(?i)("isError"\s*:\s*true|"success"\s*:\s*false)'
        Record-Verification -Command $command -Failed $failed

        if ($failed) {
            Write-Feedback 'Command failed. Fix the reported errors and rerun the same dotnet command before proceeding.' 'Prefer solution- or project-scoped dotnet commands. For harness verification, run scripts/verify.ps1 -SolutionPath <path> or -ProjectPath <path>; add -SkipBuild for fast checks.'
            exit 0
        }

        exit 0
    }

    if ($toolName -eq 'apply_patch') {
        $patch = [string]$hookPayload.tool_input.command
        if ([string]::IsNullOrWhiteSpace($patch)) {
            exit 0
        }

        $touchesRuntimeWiring = $patch -match '(?i)(Program\.cs|host\.json|\.csproj)'
        if ($touchesRuntimeWiring) {
            Write-Feedback 'Runtime wiring files changed.' 'After Program.cs/host.json/.csproj changes, run targeted tests for changed projects and then scripts/verify.ps1 -SolutionPath <path> -SkipBuild or scripts/verify.ps1 -ProjectPath <path> -SkipBuild.'
            exit 0
        }

        exit 0
    }

    exit 0
}
catch {
    # Fail open to avoid blocking normal tool flow.
    exit 0
}
