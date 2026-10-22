$ErrorActionPreference = 'Stop'

function Write-Decision([string]$behavior, [string]$message) {
    $payload = @{
        hookSpecificOutput = @{
            hookEventName = 'PermissionRequest'
            decision = @{
                behavior = $behavior
                message = $message
            }
        }
    } | ConvertTo-Json -Depth 8 -Compress

    Write-Output $payload
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
    if ([string]$hookPayload.hook_event_name -ne 'PermissionRequest') {
        exit 0
    }

    if ([string]$hookPayload.tool_name -ne 'Bash') {
        exit 0
    }

    $command = [string]$hookPayload.tool_input.command
    $description = [string]$hookPayload.tool_input.description

    if ([string]::IsNullOrWhiteSpace($command)) {
        exit 0
    }

    $denyPatterns = @(
        '(?i)\bgit\s+reset\s+--hard\b',
        '(?i)\bgit\s+checkout\s+--\b',
        '(?i)\bgit\s+clean\s+-f',
        '(?i)\brm\s+-rf\b',
        '(?i)\bdel\s+/f\s+/q\b',
        '(?i)\bformat\s+[a-z]:\b'
    )

    foreach ($pattern in $denyPatterns) {
        if ($command -match $pattern) {
            Write-Decision 'deny' 'Blocked by repository policy: destructive command cannot be approved via PermissionRequest.'
            exit 0
        }
    }

    $riskyPatterns = @(
        '(?i)\b(curl|wget|Invoke-WebRequest|Invoke-RestMethod)\b',
        '(?i)\b(npm\s+install|pnpm\s+add|yarn\s+add|pip\s+install|dotnet\s+add\s+package|choco\s+install|winget\s+install)\b'
    )

    $isRisky = $false
    foreach ($pattern in $riskyPatterns) {
        if ($command -match $pattern) {
            $isRisky = $true
            break
        }
    }

    if ($isRisky -and [string]::IsNullOrWhiteSpace($description)) {
        Write-Decision 'deny' 'Approval denied: include a short tool_input.description explaining why this network or package operation is needed.'
        exit 0
    }

    exit 0
}
catch {
    # Fail open to avoid blocking all approvals on parser/runtime errors.
    exit 0
}
