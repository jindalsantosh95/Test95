$ErrorActionPreference = 'Stop'

function Write-Deny([string]$reason) {
    $payload = @{
        hookSpecificOutput = @{
            hookEventName = 'PreToolUse'
            permissionDecision = 'deny'
            permissionDecisionReason = $reason
        }
    } | ConvertTo-Json -Depth 8 -Compress

    Write-Output $payload
}

function Write-Context([string]$message) {
    $payload = @{
        hookSpecificOutput = @{
            hookEventName = 'PreToolUse'
            additionalContext = $message
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
    $toolName = [string]$hookPayload.tool_name

    if ($toolName -eq 'Bash') {
        $command = [string]$hookPayload.tool_input.command
        if ([string]::IsNullOrWhiteSpace($command)) {
            exit 0
        }

        $destructivePatterns = @(
            '(?i)\bgit\s+reset\s+--hard\b',
            '(?i)\bgit\s+checkout\s+--\b',
            '(?i)\bgit\s+clean\s+-f',
            '(?i)\brm\s+-rf\b',
            '(?i)\bdel\s+/f\s+/q\b',
            '(?i)\bformat\s+[a-z]:\b'
        )

        foreach ($pattern in $destructivePatterns) {
            if ($command -match $pattern) {
                Write-Deny 'Blocked by repository policy: destructive shell command detected.'
                exit 0
            }
        }

        $isPrCommentReplyEndpoint = $command -match '(?i)\bgh\s+api\b.*\bpulls/comments/\d+/replies\b'
        if ($isPrCommentReplyEndpoint) {
            $isMutatingReplyCall = (
                $command -match '(?i)(^|\s)-X\s+POST(\s|$)' -or
                $command -match '(?i)(^|\s)--method\s+POST(\s|$)' -or
                $command -match '(?i)(^|\s)-f\s+body=' -or
                $command -match '(?i)(^|\s)-F\s+body='
            )

            if ($isMutatingReplyCall) {
                $hasCodexTag = $command -match '(?i)\[from-codex\]'
                if (-not $hasCodexTag) {
                    Write-Deny 'Blocked by repository policy: PR review-comment replies must include the tag [from-codex] in the comment body.'
                    exit 0
                }
            }
        }

        if ($command -match '(?i)\bdotnet\s+(build|test|restore)\b' -and $command -notmatch '(?i)\.(sln|csproj)\b') {
            $cwd = [string]$hookPayload.cwd
            if (-not [string]::IsNullOrWhiteSpace($cwd) -and (Split-Path -Leaf $cwd) -eq 'Enterprise Microservices') {
                Write-Context 'Prefer solution- or project-scoped dotnet commands from the relevant folder, or pass an explicit .sln/.csproj path. Run root harness scripts with scripts/verify.ps1 -SolutionPath <path> or -ProjectPath <path>.'
                exit 0
            }
        }

        exit 0
    }

    if ($toolName -eq 'apply_patch') {
        $patch = [string]$hookPayload.tool_input.command
        if ([string]::IsNullOrWhiteSpace($patch)) {
            exit 0
        }

        $touchesCsproj = $patch -match '(?i)(^|\s)(\*\*\* (Update|Add) File: )?.+\.csproj'
        $addsInlineVersion = $patch -match '(?i)<PackageReference[^>]*\bVersion='

        if ($touchesCsproj -and $addsInlineVersion) {
            Write-Deny 'Blocked by repository policy: do not add inline PackageReference Version attributes in project files. Use central package management where available.'
            exit 0
        }

        exit 0
    }

    exit 0
}
catch {
    # Fail-open to avoid blocking normal work if hook parsing fails.
    exit 0
}
