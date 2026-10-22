#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$tokens = $null; $errors = $null
$ast = [Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'run-sequence-goal-agent.ps1'), [ref]$tokens, [ref]$errors)
if ($errors.Count) { throw $errors[0] }
$loop = $ast.Find({ param($node) $node -is [Management.Automation.Language.DoWhileStatementAst] -and $node.Extent.Text.Contains('$continueProduction') }, $true)
if (-not $loop) { throw 'Production continuation loop not found.' }
$execute = [scriptblock]::Create($loop.Extent.Text.Replace('$PSScriptRoot', ("'" + $PSScriptRoot.Replace("'", "''") + "'")))
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('goal-session-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $fixture | Out-Null
$agentModel = 'gpt-5.6-terra'
$executable = 'Invoke-FixtureCodex'
$DeliveryPhase = 'Implementation'
$maximumInfrastructureRetries = 1
$resumeOptions = @('-c', 'model_reasoning_effort="high"')
function Invoke-FixtureCodex {
    $script:calls++
    if ($script:calls -gt 4) { throw 'Continuation did not terminate.' }
    if ($script:calls -gt 1 -and ($args -notcontains 'resume' -or $args -notcontains '11111111-1111-1111-1111-111111111111' -or $args -notcontains 'model_reasoning_effort="high"')) { throw 'Continuation lost session identity or options.' }
    $global:LASTEXITCODE = 0
    if ($script:scenario -eq 'wrong-session' -and $script:calls -gt 1) { '{"type":"thread.started","thread_id":"22222222-2222-2222-2222-222222222222"}' }
    elseif ($script:scenario -ne 'missing-session' -and -not ($script:scenario -eq 'missing-resume-session' -and $script:calls -gt 1)) { '{"type":"thread.started","thread_id":"11111111-1111-1111-1111-111111111111"}' }
    if (($script:scenario -eq 'dropped-resume' -and $script:calls -eq 2) -or ($script:scenario -eq 'repeated-drop' -and $script:calls -ge 2)) {
        '{"type":"error","message":"event stream lagged; dropped 2 events"}'
        $global:LASTEXITCODE = 1
        return
    }
    $status = if ($script:scenario -eq 'blocked') { 'Blocked' } elseif ($script:calls -gt 1 -or $script:scenario -eq 'complete') { 'Complete' } else { 'Incomplete' }
    $outcome = @{status=$status;summary='Fixture work';acceptanceEvidence=@('verified');verification=@(@{command='verify';exitCode=0});remainingWork=@();stopReason='More tests remain'}
    if ($status -ne 'Complete') { $outcome.remainingWork = @('Implement remaining tests') }
    if ($script:scenario -eq 'missing-reason') { $outcome.Remove('stopReason') }
    @{type='item.completed';item=@{type='agent_message';text=($outcome | ConvertTo-Json -Compress -Depth 5)}} | ConvertTo-Json -Compress -Depth 6
    # A $16 cached-input turn fits the current allowance, but another would exceed $30.
    $inputTokens = if ($script:scenario -eq 'budget') { 40000000 } else { 100 }
    $outputTokens = if ($script:scenario -eq 'legacy-output-limit') { if ($script:calls -eq 1) { 33803 } else { 14367 } } else { 10 }
    @{type='turn.completed';usage=@{input_tokens=$inputTokens;cached_input_tokens=$inputTokens;output_tokens=$outputTokens}} | ConvertTo-Json -Compress
}
foreach ($scenario in @('continue','legacy-output-limit','complete','blocked','budget','time','missing-session','missing-reason','wrong-session','missing-resume-session','dropped-resume','repeated-drop')) {
    $script:scenario = $scenario; $script:calls = 0
    $infrastructureRetries = 0; $sessionId = $null; $previousBudget = $null; $attestedPaths = @()
    $sessionMinutes = 40; $sessionStarted = Get-Date
    if ($scenario -eq 'time') { $sessionStarted = (Get-Date).AddMinutes(-41) }
    $UsagePath = Join-Path $fixture "$scenario.jsonl"
    $BudgetEvidencePath = Join-Path $fixture "$scenario.budget.json"
    $ProductionOutcomePath = Join-Path $fixture "$scenario.outcome.json"
    $resolvedArgs = @('exec','--json'); $agentInput = 'Implement goal'
    $failure = ''
    try { . $execute } catch { $failure = $_.Exception.Message }
    if ($scenario -eq 'missing-session') { if ($failure -notlike '*without its Codex session ID*') { throw "Wrong failure: $failure" }; continue }
    if ($scenario -eq 'missing-reason') { if ($failure -notlike '*stopReason*') { throw "Wrong failure: $failure" }; continue }
    if ($scenario -eq 'wrong-session') { if ($failure -notlike '*different session*') { throw "Wrong failure: $failure" }; continue }
    if ($scenario -eq 'missing-resume-session') { if ($failure -notlike '*did not report its session ID*') { throw "Wrong failure: $failure" }; continue }
    if ($failure) { throw $failure }
    if ($scenario -eq 'repeated-drop') {
        if ($script:calls -ne 3 -or $hasTerminalTurn -or $agentExitCode -eq 0) { throw 'Dropped-stream retry limit did not preserve terminal failure.' }
        continue
    }
    $expectedCalls = if ($scenario -eq 'dropped-resume') { 3 } elseif ($scenario -in @('continue','legacy-output-limit')) { 2 } else { 1 }
    if ($script:calls -ne $expectedCalls) { throw "Wrong invocation count for $scenario" }
    $budget = Get-Content $BudgetEvidencePath -Raw | ConvertFrom-Json
    $expectedTurns = if ($scenario -eq 'dropped-resume') { 2 } else { $expectedCalls }
    if ($budget.usage.turns -ne $expectedTurns) { throw 'Budget did not accumulate across turns.' }
    if ($scenario -eq 'legacy-output-limit' -and $budget.usage.outputTokens -ne 48170) { throw 'Affordable continuation beyond the old output limit was not retained.' }
    if ($scenario -in @('budget','time') -and $lastAgentMessage -notmatch 'monitor recovery') { throw 'Runner stopping reason was not recorded.' }
}
Write-Host 'Session continuation exercise passed: same-session completion, blocked, cumulative budget, time, missing session and stop reason.'
$global:LASTEXITCODE = 0
