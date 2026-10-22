$ErrorActionPreference = "Stop"

$temporaryDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $temporaryDirectory | Out-Null
$trxPath = Join-Path $temporaryDirectory "fixture.trx"
$manifestPath = Join-Path $temporaryDirectory "trace-manifest-book-sit.json"
$passManifestPath = Join-Path $temporaryDirectory "trace-manifest-pass.json"
$analyserPath = Join-Path $PSScriptRoot "analyse-test-run-results.ps1"
$errorId = "337186ef-aaac-4cfe-bcf7-04c1e8a45a8b"
$basketId = "4034ee0a-6f75-4625-befa-16e1453a2047"
$correlationId = "8e28a856-4c51-48bb-9b91-33fbe219b81e"
$requestId = "7cf90dfb-42ef-440a-9060-2d8c4b17c8f7"
$businessRecordId = "55e69b7f-2463-40a5-9a21-4531f4972830"
$jsonCorrelationId = "a0b81596-1cf7-4dc5-8bee-819f55c56526"
$escapedJsonRequestId = "debd2079-3ded-4197-ac11-976c820fd063"

try {
    @"
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results>
    <UnitTestResult testName="Example(scenario: Order_Create)" outcome="Failed" duration="00:00:03" startTime="2026-10-15T04:01:05Z" endTime="2026-10-15T04:01:08Z">
      <Output>
        <ErrorInfo>
          <Message>Error code was returned in GQL response.&#xD;
BasketId=$basketId&#xD;
operationName=bookCreateOrder&#xD;
ErrorID: $errorId&#xD;
correlation=$correlationId&#xD;
request=$requestId&#xD;
Response JSON: {&quot;correlationId&quot;:&quot;$jsonCorrelationId&quot;}&#xD;
Escaped response JSON: {\&quot;requestId\&quot;:\&quot;$escapedJsonRequestId\&quot;}&#xD;
Response business record: {&quot;id&quot;:&quot;$businessRecordId&quot;}&#xD;
Response: {&quot;errors&quot;:[{&quot;message&quot;:&quot;Request failed.&quot;,&quot;extensions&quot;:{&quot;code&quot;:&quot;500&quot;}}],&quot;data&quot;:{&quot;bookCreateOrder&quot;:null}}</Message>
        </ErrorInfo>
      </Output>
    </UnitTestResult>
    <UnitTestResult testName="Passing test" outcome="Passed" duration="00:00:01" startTime="2026-10-15T04:01:05Z" endTime="2026-10-15T04:01:06Z" />
  </Results>
</TestRun>
"@ | Set-Content -LiteralPath $trxPath -Encoding utf8

    & $analyserPath -TrxPath $trxPath -SuiteName "Fixture" -EnvironmentName sit -TraceManifestPath $manifestPath | Out-Null
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) { throw "Failure manifest was not created." }

    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    if ($manifest.environment -ne "sit") { throw "Manifest environment was not sourced from the analyser input." }
    if ($manifest.failureCount -ne 1 -or @($manifest.failures).Count -ne 1) { throw "Manifest must contain failed tests only." }
    if ($manifest.failures[0].operationName -ne "bookCreateOrder") { throw "GraphQL operation was not extracted. Actual='$($manifest.failures[0].operationName)' Message='$($manifest.failures[0].message)'" }
    if (@($manifest.failures[0].identifiers) -notcontains $errorId) { throw "Error identifier was not extracted." }
    if (@($manifest.failures[0].identifiers) -notcontains $correlationId) { throw "The correlation=<guid> diagnostic was not extracted." }
    if (@($manifest.failures[0].identifiers) -notcontains $requestId) { throw "The request=<guid> diagnostic was not extracted." }
    if (@($manifest.failures[0].identifiers) -notcontains $jsonCorrelationId) { throw "A quoted JSON correlationId was not extracted." }
    if (@($manifest.failures[0].identifiers) -notcontains $escapedJsonRequestId) { throw "An escaped JSON requestId was not extracted." }
    if (@($manifest.failures[0].identifiers) -contains $basketId) { throw "Basket IDs must not be treated as trace identifiers." }
    if (@($manifest.failures[0].identifiers) -contains $businessRecordId) { throw "Bare JSON IDs must not be treated as trace identifiers." }
    Write-Host "PASS: failure-only SIT manifest contains the traceable error ID"

    $prodRejected = $false
    try {
        & $analyserPath -TrxPath $trxPath -SuiteName "Fixture" -EnvironmentName prod -TraceManifestPath (Join-Path $temporaryDirectory "prod.json") | Out-Null
    }
    catch {
        $prodRejected = $_.Exception.Message -match "not permitted"
    }
    if (-not $prodRejected) { throw "Production trace manifest generation was not rejected." }
    Write-Host "PASS: production trace manifest generation is disabled"

    @"
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results><UnitTestResult testName="Passing test" outcome="Passed" /></Results>
</TestRun>
"@ | Set-Content -LiteralPath $trxPath -Encoding utf8
    & $analyserPath -TrxPath $trxPath -SuiteName "Fixture" -EnvironmentName sit -TraceManifestPath $passManifestPath | Out-Null
    if (Test-Path -LiteralPath $passManifestPath) { throw "A trace manifest was generated for a passing run." }
    Write-Host "PASS: passing runs do not generate trace manifests"
}
finally {
    Remove-Item -LiteralPath $temporaryDirectory -Recurse -Force -ErrorAction SilentlyContinue
}
