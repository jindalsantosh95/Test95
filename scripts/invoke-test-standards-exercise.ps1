#Requires -Version 5.1
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'TestStandards.psm1') -Force

$mustReject = @(
    'using Moq;',
    'global using Moq;',
    'using M = Moq;',
    'using M = global::Moq;',
    'using MockType = Moq.Mock<object>;',
    'var value = new Moq.Mock<object>();',
    'var value = new global::Moq.Mock<object>();',
    'var value = Mock(objectType);',
    'It.IsAny<string>()',
    'Times.Once()'
)
$mustAllow = @(
    'using Company.Testing;',
    'var value = new HandWrittenFake();',
    'var mockResponse = LoadResource("response.mock");'
)
$httpMustReject = @(
    'private sealed class FakeConnector : IHttpConnector { }',
    'services.AddSingleton<IHttpConnector>(new TestConnector());'
)
$httpMustAllow = @(
    'services.AddSingleton<IHttpClientFactory, VAA.EAPI.Core.Xunit.Http.HttpClientFactory>().AddSingleton<IHttpConnector, HttpConnector>();'
)

$failures = New-Object System.Collections.Generic.List[string]
foreach ($content in $mustReject) {
    if (-not (Test-ContainsMoqUsage -Content $content)) {
        $failures.Add("Detector allowed Moq syntax: $content") | Out-Null
    }
}
foreach ($content in $mustAllow) {
    if (Test-ContainsMoqUsage -Content $content) {
        $failures.Add("Detector rejected non-Moq syntax: $content") | Out-Null
    }
}
foreach ($content in $httpMustReject) {
    if (-not (Test-ContainsAdHocHttpConnectorUsage -Content $content)) {
        $failures.Add("Detector allowed ad hoc HTTP connector test code: $content") | Out-Null
    }
}
foreach ($content in $httpMustAllow) {
    $containsAdHocConnector = Test-ContainsAdHocHttpConnectorUsage -Content $content
    $containsSharedPipeline = Test-ContainsSharedHttpTestPipeline -Content $content
    if ($containsAdHocConnector -or -not $containsSharedPipeline) {
        $failures.Add("Detector rejected or failed to recognise shared HTTP test pipeline: $content") | Out-Null
    }
}

if ($failures.Count -gt 0) {
    Write-Host 'test standards exercise failed:' -ForegroundColor Red
    $failures | ForEach-Object { Write-Host "- $_" }
    exit 1
}

Write-Host "test standards exercise passed ($($mustReject.Count + $mustAllow.Count + $httpMustReject.Count + $httpMustAllow.Count) cases)" -ForegroundColor Green
exit 0
