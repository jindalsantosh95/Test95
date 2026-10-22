[CmdletBinding()]
param(
    [ValidateSet('Sanity', 'Regression')][string]$Pack = 'Sanity',
    [string]$TestName = '',
    [string]$TrxPath,
    [string]$GithubOutputPath
)
$ErrorActionPreference = 'Stop'
$root = Join-Path $PSScriptRoot '../VAA.EAPI.Automation/VAA.EAPI.Automation.MobileApp.UI'
$contract = Get-Content -Raw -LiteralPath (Join-Path $root 'Contracts/search-book-automation-contract.json') | ConvertFrom-Json
$manifest = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../config/automation-suites/mobile-search-book-sanity.json') | ConvertFrom-Json
$ids = @($contract.releasePacks.sanity)
$filter = $manifest.testFilter
$selectedPack = $Pack
if ($Pack -ne 'Sanity') {
    $ids = @($contract.releasePacks.regression)
    $filter = 'Suite=Regression&RegressionArea=SearchAndBook&Platform=iOS'
}
if (-not [string]::IsNullOrEmpty($TestName)) {
    if ($Pack -ne 'Sanity') { throw 'Choose a pack or an exact test name, not both.' }
    if ($TestName -cnotmatch '^(?:Loyalty)?SearchBook_TC[0-9]+_[A-Za-z0-9_]+$') { throw 'Use an exact test method name, not a filter or partial name.' }
    $allowed = @($contract.releasePacks.sanity) + @($contract.releasePacks.regression)
    $scenario = @($contract.scenarios | Where-Object { $_.testMethod -ceq $TestName -and $_.id -in $allowed })
    if ($scenario.Count -ne 1) { throw 'Test must be a unique scenario in the iOS Sanity or Regression release pack; deferred tests are not supported here.' }
    $ids = @($scenario[0].id)
    $methodPattern = '\bpublic\s+(?:async\s+)?(?:void|Task)\s+' + [regex]::Escape($TestName) + '\s*\('
    $definitions = @(Get-ChildItem -LiteralPath (Join-Path $root 'Tests') -Filter '*.cs' -Recurse | Where-Object {
        (Get-Content -Raw -LiteralPath $_.FullName) -match $methodPattern
    })
    if ($definitions.Count -ne 1) { throw 'Cannot resolve one concrete test definition.' }
    $source = Get-Content -Raw -LiteralPath $definitions[0].FullName
    $methodMatches = [regex]::Matches($source, $methodPattern)
    if ($methodMatches.Count -ne 1) { throw 'Cannot resolve one test method declaration.' }
    # Numbered mobile tests use file-scoped namespaces and top-level classes.
    # A file may contain a base class and several sibling scenario classes.
    # Select the last top-level declaration before the method, not the first in the file.
    $namespace = [regex]::Match($source, '(?m)^namespace\s+([A-Za-z0-9_.]+)\s*;')
    $classes = @([regex]::Matches($source.Substring(0, $methodMatches[0].Index), '(?m)^public\s+(?:(sealed|abstract)\s+)?class\s+([A-Za-z0-9_]+)'))
    if (-not $namespace.Success -or $classes.Count -eq 0) { throw 'Expected a file-scoped namespace and top-level test class.' }
    $class = $classes[-1]
    if ($class.Groups[1].Value -eq 'abstract') { throw 'Selected method must belong to a concrete scenario class.' }
    $filter = "FullyQualifiedName=$($namespace.Groups[1].Value).$($class.Groups[2].Value).$TestName&RegressionArea=SearchAndBook&Platform=iOS"
    $selectedPack = 'Targeted'
}
$methods = @($ids | ForEach-Object {
    $id = $_
    $matches = @($contract.scenarios | Where-Object { $_.id -ceq $id })
    if ($matches.Count -ne 1) { throw 'Invalid release-pack scenario mapping.' }
    $matches[0].testMethod
})
if ($methods.Count -eq 0) { throw 'Empty selection is not allowed.' }
if ($TrxPath) {
    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $reader = [System.Xml.XmlReader]::Create((Resolve-Path -LiteralPath $TrxPath).Path, $settings)
    try {
        $xml = [System.Xml.XmlDocument]::new()
        $xml.XmlResolver = $null
        $xml.Load($reader)
    } finally { $reader.Dispose() }
    $results = @($xml.SelectNodes("/*[local-name()='TestRun']/*[local-name()='Results']/*[local-name()='UnitTestResult']"))
    $actual = @($results | ForEach-Object { ([string]$_.testName -split '\.')[-1] })
    if ($results.Count -ne $methods.Count -or @($results | Where-Object { $_.outcome -ne 'Passed' }).Count -gt 0 -or
        @($actual | Select-Object -Unique).Count -ne $methods.Count -or
        @($methods | Where-Object { $_ -cnotin $actual }).Count -gt 0) {
        throw 'Run did not pass exactly the selected scenarios; missing, extra, duplicate, skipped or failed results are not accepted.'
    }
}
if ($GithubOutputPath) {
    @("filter=$filter", "pack=$selectedPack", "expected=$($methods.Count)") | Out-File -LiteralPath $GithubOutputPath -Append -Encoding utf8
}
[pscustomobject]@{ Filter = $filter; Pack = $selectedPack; Expected = $methods.Count; Methods = $methods }
