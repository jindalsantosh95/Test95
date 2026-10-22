$ErrorActionPreference = 'Stop'

try {
    $raw = [Console]::In.ReadToEnd()
    if ([string]::IsNullOrWhiteSpace($raw)) {
        $raw = ($input | Out-String)
    }

    if ([string]::IsNullOrWhiteSpace($raw)) {
        exit 0
    }

    $hookPayload = $raw | ConvertFrom-Json

    if ([string]$hookPayload.hook_event_name -ne 'Stop') {
        exit 0
    }

    if ($hookPayload.stop_hook_active -eq $true) {
        # Allow normal stop after one continuation reminder.
        exit 0
    }

    $cwd = [string]$hookPayload.cwd
    if ([string]::IsNullOrWhiteSpace($cwd)) {
        exit 0
    }

    $repoRootRaw = (& git -C $cwd rev-parse --show-toplevel 2>$null).Trim()
    $repoRoot = if ([string]::IsNullOrWhiteSpace($repoRootRaw)) { '' } else { (Resolve-Path -LiteralPath $repoRootRaw).Path }
    if ([string]::IsNullOrWhiteSpace($repoRoot)) {
        exit 0
    }

    $statePath = Join-Path $repoRoot '.codex/state/session-state.json'
    try {
        $lifecycle = Join-Path $repoRoot 'scripts/update-agent-memory-state.ps1'
        & $lifecycle -Action Checkpoint -NextActions 'Review the worktree state and continue or finalize the active objective' -Actor 'codex-stop' *> $null
    } catch {
        # The reminder below reports invalid state without hiding verification guidance.
    }

    $repoChanges = @(
        & git -C $repoRoot status --porcelain 2>$null |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )

    if ($repoChanges.Count -eq 0) {
        exit 0
    }

    $memoryReminder = 'Optionally update the untracked local .codex/state/session-state.json for same-worktree resumption; do not commit it.'
    if (Test-Path -LiteralPath $statePath -PathType Leaf) {
        try {
            $state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
            $memoryReminder = "Local memory was checkpointed automatically (revision '$($state.revision)', status '$($state.status)', updatedAtUtc '$($state.updatedAtUtc)'); it remains ignored and must not be committed."
        } catch {
            $memoryReminder = 'The optional local .codex/state/session-state.json is invalid; fix or delete it before finalizing, and do not commit it.'
        }
    }

    $payload = @{
        decision = 'block'
        reason = "Before finalizing repository changes, run scripts/verify.ps1 -SolutionPath <path> -SkipBuild (or -ProjectPath <path>) for the changed scope and include the result. $memoryReminder"
    } | ConvertTo-Json -Depth 6 -Compress

    Write-Output $payload
    exit 0
}
catch {
    # Fail open if hook parsing or repo probing fails.
    exit 0
}
