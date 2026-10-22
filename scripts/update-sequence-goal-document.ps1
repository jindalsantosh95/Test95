#Requires -Version 7.0
# A short optimistic Git transaction. Reapply the edit to the latest document on
# contention; never force-push or copy a stale whole document over another writer.
function Update-SequenceGoalDocument {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$RepositoryRoot,
        [Parameter(Mandatory)][string]$Branch,
        [Parameter(Mandatory)][ValidatePattern('^docs/sequence-diagrams/(?!_)[A-Za-z0-9_.-]+\.md$')][string]$DiagramPath,
        [Parameter(Mandatory)][scriptblock]$Edit,
        [Parameter(Mandatory)][string]$Message,
        [switch]$IncludeIndex,
        [ValidateRange(1, 10)][int]$Attempts = 5
    )
    $ErrorActionPreference = 'Stop'
    if ($Branch -eq 'main') { throw 'Delivery progress must be written to a goal branch, not main.' }
    $authArguments = @()
    $token = if ($env:COORDINATOR_GH_TOKEN) { $env:COORDINATOR_GH_TOKEN } else { $env:GH_TOKEN }
    if ($token) {
        $header = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("x-access-token:$token"))
        # extraheader is multi-valued: -c adds to checkout/environment credentials.
        # Reset that list for this command before selecting the coordinator token.
        $authArguments = @('-c', 'http.https://github.com/.extraheader=', '-c', "http.https://github.com/.extraheader=AUTHORIZATION: basic $header")
    }
    git check-ref-format --branch $Branch | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Invalid goal branch.' }
    for ($attempt = 1; $attempt -le $Attempts; $attempt++) {
        git @authArguments -C $RepositoryRoot fetch --no-tags origin "refs/heads/$Branch" | Out-Host
        if ($LASTEXITCODE -ne 0) { throw 'Cannot fetch the goal branch.' }
        $base = (git -C $RepositoryRoot rev-parse FETCH_HEAD).Trim()
        $worktree = Join-Path ([IO.Path]::GetTempPath()) "goal-update-$([guid]::NewGuid().ToString('N'))"
        git -C $RepositoryRoot worktree add --detach $worktree $base | Out-Host
        if ($LASTEXITCODE -ne 0) { throw 'Cannot prepare goal update.' }
        try {
            Push-Location $worktree
            try {
                foreach ($path in @('docs', 'docs/sequence-diagrams', $DiagramPath) + @('docs/sequence-diagrams.md' | Where-Object { $IncludeIndex })) {
                    $entry = Get-Item -LiteralPath $path -Force -ErrorAction Stop
                    if ($entry.LinkType -or ($entry.Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'Goal path cannot contain links.' }
                }
                & $Edit $DiagramPath
                $changed = @(git status --porcelain)
                if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect goal update.' }
                if ($changed.Count -eq 0) { return }
                $allowed = @($DiagramPath) + @('docs/sequence-diagrams.md' | Where-Object { $IncludeIndex })
                if (@($changed | Where-Object { $_.Substring(3) -cnotin $allowed }).Count) { throw 'Goal update must change only the selected document and its index.' }
                git add -- @allowed
                if ($LASTEXITCODE -ne 0) { throw 'Cannot stage goal update.' }
                git -c core.hooksPath=/dev/null -c user.name=github-actions[bot] -c user.email=github-actions[bot]@users.noreply.github.com commit -m $Message | Out-Host
                if ($LASTEXITCODE -ne 0) { throw 'Cannot commit goal update.' }
                git @authArguments push origin "HEAD:refs/heads/$Branch" | Out-Host
                if ($LASTEXITCODE -eq 0) { return }
                $remote = @(git @authArguments ls-remote --heads origin "refs/heads/$Branch")
                if ($LASTEXITCODE -ne 0 -or $remote.Count -ne 1 -or ($remote[0] -split '\s+')[0] -eq $base) {
                    throw 'Goal push failed without a concurrent update; check repository access.'
                }
            } finally { Pop-Location }
        } finally {
            # Only this function's disposable worktree is removed. The caller is untouched.
            git -C $RepositoryRoot worktree remove --force $worktree | Out-Host
        }
    }
    throw "Goal document remained busy after $Attempts attempts; published implementation is safe to resume."
}
