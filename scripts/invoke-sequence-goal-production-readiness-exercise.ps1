#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'sequence-goal-production-recovery.ps1')
$cases = 0
function Assert-Rejected {
    param([string]$Message)
    $rejected = $false
    try { ConvertFrom-ProductionOutcome $Message | Out-Null } catch { $rejected = $true }
    if (-not $rejected) { throw "Unsafe production outcome accepted: $Message" }
    $script:cases++
}
# A clean process exit with the original prose refusal is not production success.
Assert-Rejected 'Blocked - only the facade skeleton is implemented.'
Assert-Rejected '{}'
Assert-Rejected '{"status":"Complete","summary":"done","acceptanceEvidence":[],"remainingWork":[],"verification":[]}'
Assert-Rejected '{"status":"Complete","summary":"done","acceptanceEvidence":["tested"],"remainingWork":["subscriber missing"],"verification":[{"command":"verify","exitCode":0}]}'
Assert-Rejected '{"status":"Complete","summary":"done","acceptanceEvidence":["tested"],"remainingWork":[],"verification":[{"command":"verify","exitCode":1}]}'
Assert-Rejected '{"status":"Complete","summary":"done","acceptanceEvidence":["tested"],"remainingWork":[],"verification":[{"command":"verify"}]}'
Assert-Rejected '{"status":"Complete","summary":"done","acceptanceEvidence":[""],"remainingWork":[],"verification":[{"command":"verify","exitCode":0}]}'
foreach ($status in @('Incomplete', 'Blocked')) {
    $outcome = ConvertFrom-ProductionOutcome (@{ status=$status; summary='Subscriber missing'; acceptanceEvidence=@('Validator tests passed'); remainingWork=@('Implement consumer'); verification=@() } | ConvertTo-Json)
    if ($outcome.status -ne $status -or $outcome.remainingWork[0] -ne 'Implement consumer') { throw 'Partial production evidence was lost.' }
    $cases++
}
$complete = ConvertFrom-ProductionOutcome '{"status":"Complete","summary":"Implemented and verified","acceptanceEvidence":["All acceptance branches tested"],"remainingWork":[],"verification":[{"command":"verify scoped project","exitCode":0}]}'
if ($complete.status -ne 'Complete') { throw 'Valid completion rejected.' }
$cases++
$head = 'a' * 40
$goalDigest = 'c' * 64
foreach ($state in @($null, @{status='InProgress';verifiedCommit=$head}, @{status='Complete';verifiedCommit=('b'*40)})) {
    if (Test-ProductionReadiness $state $head) { throw 'Missing or stale readiness accepted.' }
    $cases++
}
if (-not (Test-ProductionReadiness @{status='Complete';verifiedCommit=$head;identity=$goalDigest;currentGoalDigest=$goalDigest} $head)) { throw 'Current completion rejected.' }
foreach ($digest in @('', ('d' * 64))) {
    if (Test-ProductionReadiness @{status='Complete';verifiedCommit=$head;identity=$goalDigest;currentGoalDigest=$digest} $head) { throw 'Missing or changed goal requirements accepted.' }
    $cases++
}
$cases++
# Compile workflow PowerShell as well: this catches control-flow/bracing regressions
# in the resumed-PR path without invoking GitHub or any delivery agent.
$root = Split-Path $PSScriptRoot -Parent
$parsedBlocks = 0
$testedResume = $false
$testedPublication = $false
foreach ($name in @('ai-delivery-sequence-goal-loop.yml', 'ai-delivery-sequence-goal-qa.yml')) {
    $workflow = Get-Content -LiteralPath (Join-Path $root ".github/workflows/$name") -Raw
    if ($name -eq 'ai-delivery-sequence-goal-qa.yml') {
        $completionGate = [regex]::Match($workflow, 'Record QA delivery PRs in goal diagrams\r?\n\s+if: (?<condition>[^\r\n]+)').Groups['condition'].Value
        if (-not $completionGate -or $workflow.IndexOf('Record QA delivery PRs in goal diagrams') -lt $workflow.IndexOf('Build generated automation tests')) { throw 'QA completion precedes mandatory build verification.' }
        $completionGate = $completionGate.Replace('env.QA_HAS_INPROGRESS', '$active').Replace('steps.automation_agent.outcome', '$agentResult').Replace('steps.generated_automation_restore.outcome', '$restoreResult').Replace('steps.automation_build.outcome', '$buildResult').Replace('&&', '-and').Replace('==', '-eq')
        $active = 'true'; $agentResult = 'success'
        foreach ($restoreResult in @('success','failure','skipped')) {
            foreach ($buildResult in @('success','failure','skipped')) {
                $canComplete = & ([scriptblock]::Create($completionGate))
                if ($canComplete -ne ($restoreResult -eq 'success' -and $buildResult -eq 'success')) { throw 'QA completion accepts failed or skipped verification.' }
                $cases++
            }
        }
    }
    foreach ($block in [regex]::Matches($workflow, '(?ms)^        shell: pwsh\r?\n(?:(?!^      - name:).)*?^        run: \|\r?\n(?<code>(?:^          [^\r\n]*\r?\n|^\r?\n)+)')) {
        $code = $block.Groups['code'].Value -replace '(?m)^          ', '' -replace '\$\{\{.*?\}\}', 'workflow-expression'
        $tokens = $null; $errors = $null
        [void][System.Management.Automation.Language.Parser]::ParseInput($code, [ref]$tokens, [ref]$errors)
        if ($errors.Count) { throw "$name workflow PowerShell parse errors: $($errors.Message -join '; ')" }
        $parsedBlocks++
        if ($name -eq 'ai-delivery-sequence-goal-loop.yml' -and $code.Contains('$resumeProduction =')) {
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($code, [ref]$tokens, [ref]$errors)
            $assignment = $ast.Find({ param($node) $node -is [System.Management.Automation.Language.AssignmentStatementAst] -and $node.Left.Extent.Text -eq '$resumeProduction' }, $true)
            function Read-DeliveryGoalState { $script:exerciseState }
            foreach ($status in @('InProgress','Blocked','Complete')) {
                $script:exerciseState = @{status=$status;verifiedCommit=$head;identity=$goalDigest;currentGoalDigest=$goalDigest}
                $existing = @([pscustomobject]@{ body='irrelevant'; headRefOid=$head })
                . ([scriptblock]::Create($assignment.Extent.Text))
                if ($resumeProduction -ne ($status -ne 'Complete')) { throw 'Wrong resume decision from document state.' }
                $cases++
            }
            Remove-Item Function:Read-DeliveryGoalState
            $testedResume = $true
            $reuseBranch = $ast.Find({ param($node) $node -is [System.Management.Automation.Language.StatementBlockAst] -and $node.Extent.Text.Contains('$pr = $existing[0]') -and $node.Extent.Text.Contains('$recoveryMode = Get-ProductionRecoveryMode') -and -not $node.Extent.Text.Contains('$evidenceDir =') }, $true)
            if ($null -eq $reuseBranch) { throw 'Existing production branch does not preserve PR identity.' }
            $pr = $null; $resourceUsage = $null
            $existing = @([pscustomobject]@{ number=123; url='https://example.invalid/pr/123'; state='OPEN' })
            $productionState = [pscustomobject]@{ resourceUsage=@{ actualLabel='retained usage' } }
            foreach ($variable in @('$pr','$resourceUsage')) {
                $preserved = $reuseBranch.Find({ param($node) $node -is [System.Management.Automation.Language.AssignmentStatementAst] -and $node.Left.Extent.Text -eq $variable }, $true)
                . ([scriptblock]::Create($preserved.Extent.Text))
            }
            $recordExpression = $ast.Find({ param($node) $node -is [System.Management.Automation.Language.HashtableAst] -and $node.Extent.Text.Contains('number = [int]$pr.number') }, $true)
            $record = . ([scriptblock]::Create($recordExpression.Extent.Text))
            if ($record.number -ne 123 -or $record.url -ne $existing[0].url -or $record.state -ne 'OPEN' -or $record.resourceUsage.actualLabel -ne 'retained usage') { throw 'Reused production lost its ledger identity or resource evidence.' }
            $cases++
            $publication = $ast.Find({ param($node) $node -is [System.Management.Automation.Language.IfStatementAst] -and $node.Clauses[0].Item1.Extent.Text.Contains('$null -ne $resourceUsage') -and $node.Extent.Text.Contains('gh pr edit') }, $true)
            if ($null -eq $publication) { throw 'Missing production publication block.' }
            foreach ($mode in @('Resume','Publish','Reuse')) {
                $recoveryMode = $mode
                $shouldPublish = . ([scriptblock]::Create($publication.Clauses[0].Item1.Extent.Text))
                if ($shouldPublish -ne ($mode -ne 'Reuse')) { throw "Incorrect publication gate for $mode." }
                $cases++
            }
            # Run the entire actual publication block with retained resources and no
            # outcome, as on a fresh runner reusing an already-published checkpoint.
            $recoveryMode = 'Reuse'; $productionOutcome = $null
            function gh { throw 'Reuse must not edit or demote the production PR.' }
            function git { throw 'Reuse must not perform publication Git operations.' }
            try { . ([scriptblock]::Create($publication.Extent.Text)) }
            finally { Remove-Item Function:gh; Remove-Item Function:git }
            $cases++
            $reservationAssignment = $ast.Find({ param($node) $node -is [System.Management.Automation.Language.AssignmentStatementAst] -and $node.Left.Extent.Text -eq '$reservation' }, $true)
            foreach ($previous in @($null, [pscustomobject]@{ outcome=[pscustomobject]@{remainingWork=@('Implement consumer')} })) {
                $productionState = $previous
                . ([scriptblock]::Create($reservationAssignment.Extent.Text))
                $validated = ConvertFrom-ProductionOutcome ($reservation | ConvertTo-Json -Depth 8)
                if ($validated.remainingWork.Count -ne 1 -or ($null -ne $previous -and $validated.remainingWork[0] -ne 'Implement consumer')) { throw 'Attempt reservation lost remaining criteria.' }
                $cases++
            }
            $gate = $ast.Find({ param($node) $node -is [System.Management.Automation.Language.IfStatementAst] -and $node.Clauses[0].Item1.Extent.Text -eq '$productionOutcome.status -ne ''Complete''' -and $node.Extent.Text.Contains('Progress saved in the goal document') }, $true)
            if ($null -eq $gate) { throw 'Missing pre-QA partial-publication gate.' }
            function gh { $script:drafted = $args -contains '--undo'; $global:LASTEXITCODE = 0 }
            $pr = [pscustomobject]@{ number=123 }
            $livePr = [pscustomobject]@{ isDraft=$false }
            foreach ($status in @('Incomplete', 'Blocked', 'Complete')) {
                $productionOutcome = [pscustomobject]@{ status=$status; summary='test'; remainingWork=@('consumer') }
                $productionState = [pscustomobject]@{ outcome=$productionOutcome; progress=$false; attempt=1 }
                $script:drafted = $false; $stopped = $false
                try { & ([scriptblock]::Create($gate.Extent.Text + "
'continued'")) | ForEach-Object { if ($_ -eq 'continued') { $stopped = $true } } } catch { throw 'Incomplete is a normal state, not a workflow failure.' }
                if ($stopped -ne ($status -eq 'Complete') -or $script:drafted -ne ($status -ne 'Complete')) { throw 'Production gate allowed partial delivery or blocked completed delivery.' }
                $cases++
            }
            Remove-Item Function:gh
            $testedPublication = $true
        }
    }
}
if (-not $testedResume -or -not $testedPublication -or $parsedBlocks -lt 3) { throw 'Workflow regression exercise did not execute its required paths.' }
# Run the dispatcher with authenticated PR fixtures and the real document reader.
# A QA record has its own schema; it does not carry a production identity field.
$requirements = "# Goal`n`n## Agent Loop Goal`n| Goal | Protect account |`n| Harness Sensor | pending |`n| Acceptance evidence | pending |`n`n## Requirements`nLock atomically."
$semanticDigest = Get-ProductionGoalDigest $requirements
if ((Get-ProductionGoalDigest ($requirements.Replace('pending','published tests'))) -cne $semanticDigest) { throw 'Evidence publication invalidated requirements.' }
if ((Get-ProductionGoalDigest ($requirements.Replace('Lock atomically.','Lock and notify.'))) -ceq $semanticDigest) { throw 'Real requirements change did not invalidate verification.' }
$global:qaDispatchHead = 'b'*40
$global:qaDispatchPr = @{state='OPEN';isDraft=$false;mergedAt=$null;url='https://github.com/VAAD1ST/eapi/pull/42';headRefOid=$global:qaDispatchHead;author=@{login='github-actions[bot]'};headRepository=@{nameWithOwner='VAAD1ST/eapi'};baseRefName='main';body=(
    'Associated sequence-goal delivery for `VAAD1ST/eapi@goal`.' + "`n" + '- Phase: `QA`' + "`n" + '- Surface: `Backend`' + "`n" + '- Production repository: `VAAD1ST/eapi`' + "`n" + '- Production PR: `7`' + "`n" + '- Production head SHA: `' + $head + '`')}
function global:gh {
    $global:LASTEXITCODE = 0
    switch ("$($args[0]) $($args[1])") {
        'pr view' { @{state='OPEN';headRefOid=('a'*40)} | ConvertTo-Json }
        'pr list' { ConvertTo-Json -InputObject @($global:qaDispatchPr) -Depth 6 }
        'run list' {
            if ($global:qaForbidRunLookup) { throw 'Verified publication must not inspect run history.' }
            '[]'
        }
        'workflow run' { $global:qaDispatchCount++ }
        'pr ready' { $global:qaReadyCount++ }
        default {
            if ($args[0] -ne 'api') { throw "Unexpected dispatch command: $args" }
            if ($args[1] -match '/compare/') { '{"files":[{"filename":"docs/sequence-diagrams/test.md"}]}' }
            else { @{encoding='base64';content=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($global:qaDispatchDocument))} | ConvertTo-Json }
        }
    }
}
try {
    foreach ($scenario in @('current','evidence-update','qa-moved','production-binding-changed','verified-draft','publish-only')) {
        $global:qaForbidRunLookup = $scenario -eq 'publish-only'
        $text = if ($scenario -eq 'evidence-update') { $requirements.Replace('pending','published tests') } else { $requirements }
        $text = Set-GoalTaskState $text 'Backend Production' @{status='Complete';identity=$semanticDigest;verifiedCommit=$head}
        $qa = @{status='Complete';productionCommit=$head;verifiedCommit=$head;verifiedQaCommit=$global:qaDispatchHead;goalDigest=$semanticDigest;pullRequest=$global:qaDispatchPr.url}
        if ($scenario -eq 'qa-moved') { $qa.verifiedQaCommit='c'*40 }
        if ($scenario -eq 'production-binding-changed') { $qa.productionCommit='c'*40 }
        $global:qaDispatchDocument = Set-GoalTaskState $text 'Backend QA' $qa
        $global:qaDispatchCount = 0
        $global:qaReadyCount = 0
        $global:qaDispatchPr.isDraft = $scenario -in @('verified-draft','publish-only')
        & "$PSScriptRoot/dispatch-sequence-goal-qa.ps1" -Repository VAAD1ST/eapi -CoordinatorBranch goal -Surface Backend -ProductionRepository VAAD1ST/eapi -ProductionNumber 7 -PublishVerifiedOnly:($scenario -eq 'publish-only')
        $expected = if ($scenario -in @('current','evidence-update','verified-draft','publish-only')) { 0 } else { 1 }
        if ($global:qaDispatchCount -ne $expected) { throw "Wrong QA reuse decision: $scenario" }
        if ($global:qaReadyCount -ne [int]($scenario -in @('verified-draft','publish-only'))) { throw "Wrong QA ready transition: $scenario" }
        $cases++
    }
    $qa.status = 'InProgress'
    $global:qaDispatchDocument = Set-GoalTaskState $text 'Backend QA' $qa
    $global:qaReadyCount = 0; $global:qaDispatchCount = 0
    $rejected = $false
    try { & "$PSScriptRoot/dispatch-sequence-goal-qa.ps1" -Repository VAAD1ST/eapi -CoordinatorBranch goal -Surface Backend -ProductionRepository VAAD1ST/eapi -ProductionNumber 7 -PublishVerifiedOnly } catch { $rejected = $_.Exception.Message -like 'QA readiness publication requires*' }
    if (-not $rejected -or $global:qaReadyCount -or $global:qaDispatchCount) { throw 'Unverified QA was published or dispatched by the readiness-only gate.' }
} finally { Remove-Item Function:global:gh }
# Execute the resolver's actual merged-child gate: merge alone cannot suppress
# required production/QA work when verification is missing or stale.
$resolverText = Get-Content "$PSScriptRoot/resolve-sequence-goal-delivery-agents.ps1" -Raw
$resolverAst = [System.Management.Automation.Language.Parser]::ParseInput($resolverText, [ref]$null, [ref]$null)
$mergedGate = $resolverAst.Find({ param($node) $node -is [System.Management.Automation.Language.IfStatementAst] -and $node.Extent.Text.Contains('Test-QaReadiness') -and $node.Extent.Text.Contains('completedQaSurfaces.Add') }, $true)
if (-not $mergedGate) { throw 'Resolver merged verification gate is missing.' }
foreach ($phase in @('Production','QA')) {
    foreach ($scenario in @('current','stale-head','stale-requirements','missing-state')) {
        $expectedSurface='Backend'; $ledgerSurface=if ($phase -eq 'QA') { 'QA-Backend' } else { 'Backend' }
        $productionCheckpoint=@{headSha=$head}
        $current=@{headRefOid=$(if ($phase -eq 'QA') { 'b'*40 } else { $head })}
        $goalDigest=$semanticDigest
        $record=if ($phase -eq 'QA') { @{status='Complete';productionCommit=$head;verifiedCommit=$head;verifiedQaCommit=$current.headRefOid;goalDigest=$goalDigest} } else { @{status='Complete';verifiedCommit=$head;identity=$goalDigest} }
        if ($scenario -eq 'stale-head') { $record.verifiedCommit='c'*40 }
        if ($scenario -eq 'stale-requirements') { $goalDigest='d'*64 }
        $text=if ($scenario -eq 'missing-state') { $requirements } else { Set-GoalTaskState $requirements "Backend $phase" $record }
        $completedQaSurfaces=[System.Collections.Generic.HashSet[string]]::new()
        $completedProductionSurfaces=[System.Collections.Generic.HashSet[string]]::new()
        foreach ($iteration in @(1)) { . ([scriptblock]::Create($mergedGate.Extent.Text)) }
        if (($completedQaSurfaces.Count+$completedProductionSurfaces.Count) -ne [int]($scenario -eq 'current')) { throw "Merged $phase incorrectly routed: $scenario" }
        $cases++
    }
}
. (Join-Path $PSScriptRoot 'sequence-goal-documentation-maintenance.ps1')
$maintenancePr = @{ headRefName='docs-test'; baseRefName='main'; isCrossRepository=$false; labels=@(@{name='sequence-goal:documentation-only'}) }
if (-not (Test-SequenceGoalDocumentationMaintenance @($maintenancePr) 'docs-test' @('docs/sequence-diagrams/a.md', 'scripts/check.ps1', '.github/workflows/check.yml'))) { throw 'Explicit documentation maintenance was not recognized.' }
$cases++
foreach ($scenario in @('unlabelled', 'fork', 'wrong-branch', 'wrong-base')) {
    $candidate = $maintenancePr.Clone()
    switch ($scenario) {
        'unlabelled' { $candidate.labels=@() }
        'fork' { $candidate.isCrossRepository=$true }
        'wrong-branch' { $candidate.headRefName='other' }
        'wrong-base' { $candidate.baseRefName='release' }
    }
    if (Test-SequenceGoalDocumentationMaintenance @($candidate) 'docs-test' @('docs/a.md')) { throw "Unsafe documentation intent accepted: $scenario" }
    $cases++
}
foreach ($path in @('VAA.EAPI.GraphQL/App.cs', 'scripts/nested/App.cs', '.github/actions/action.yml', 'docs/payload.exe')) {
    $rejected = $false
    try { Test-SequenceGoalDocumentationMaintenance @($maintenancePr) 'docs-test' @('docs/a.md', $path) | Out-Null } catch { $rejected=$true }
    if (-not $rejected) { throw "Documentation-only intent accepted unsupported file: $path" }
    $cases++
}
$loopWorkflow = Get-Content (Join-Path $root '.github/workflows/ai-delivery-sequence-goal-loop.yml') -Raw
if ($loopWorkflow -notmatch 'documentation_only: \$\{\{ steps.route.outputs.documentation_only \}\}' -or
    $loopWorkflow -notmatch "if: needs.resolve-delivery-agents.outputs.documentation_only != 'true' &&") { throw 'Documentation maintenance no longer gates coordinator dispatch.' }
$cases++
Write-Host "Production readiness exercise passed ($cases cases; $parsedBlocks workflow PowerShell blocks parsed)."
exit 0
