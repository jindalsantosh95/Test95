$ErrorActionPreference = 'Stop'

$inputJson = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($inputJson)) {
    $inputJson = ($input | Out-String)
}
if ([string]::IsNullOrWhiteSpace($inputJson)) {
    exit 0
}

$repoRootRaw = (& git rev-parse --show-toplevel 2>$null).Trim()
$repoRoot = if ([string]::IsNullOrWhiteSpace($repoRootRaw)) { $null } else { (Resolve-Path -LiteralPath $repoRootRaw).Path }
$statePath = if ($null -ne $repoRoot) { Join-Path $repoRoot '.codex/state/session-state.json' } else { $null }

$memoryContext = ''
if ($null -ne $repoRoot) {
    try {
        $lifecycle = Join-Path $repoRoot 'scripts/update-agent-memory-state.ps1'
        & $lifecycle -Action Resume -ScopeKind ScopePath -ScopePath '.' -Actor 'codex-session-start' *> $null
        if ($LASTEXITCODE -ne 0) { throw 'memory lifecycle returned a failure' }
    } catch {
        $memoryContext = ' Memory lifecycle could not initialize or resume local state; run scripts/check-agent-memory-state.ps1 for remediation.'
    }
}
if ($null -ne $statePath -and (Test-Path -LiteralPath $statePath -PathType Leaf)) {
    try {
        $state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
        $next = @($state.nextActions) | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) } | Select-Object -First 3
        $nextText = if ($next.Count -gt 0) { ($next -join '; ') } else { 'none listed' }
        $memoryContext = " Memory snapshot: sessionId='$($state.sessionId)'; revision='$($state.revision)'; objective='$($state.objective)'; status='$($state.status)'; scope='$($state.scope.kind) $($state.scope.path)'; next='$nextText'; updatedAtUtc='$($state.updatedAtUtc)'."
    } catch {
        $memoryContext = ' Memory snapshot unavailable: .codex/state/session-state.json is present but invalid.'
    }
}

# Add stable repo-specific context for Codex sessions started in this project.
$output = @{
    hookSpecificOutput = @{
        hookEventName = 'SessionStart'
        additionalContext = "This workspace is a .NET monorepo. Prefer solution- or project-scoped dotnet commands and avoid building/testing from the repo root without an explicit .sln/.csproj. Root harness files live under docs, scripts, .githooks, .codex, and .github/copilot-instructions.md. Run scripts/verify.ps1 with -SolutionPath <path> or -ProjectPath <path> for the changed scope. Avoid destructive git commands (for example git reset --hard, git checkout --, git clean -fd). Do not add inline PackageReference Version attributes in project files when central package management is available.$memoryContext"
    }
} | ConvertTo-Json -Depth 8 -Compress

Write-Output $output
exit 0
