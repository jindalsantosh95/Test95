$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "mobile-local-settings-validation.ps1")

function Assert-Missing {
    param($Settings, [string[]]$Expected)
    $actual = @(Get-MobileLocalMissingSettings -Settings $Settings)
    if (($actual -join "|") -cne ($Expected -join "|")) {
        throw "Unexpected missing-setting names: $($actual -join ', ')"
    }
}

$ios = @{
    ServerUrl = "http://127.0.0.1:4723"; DeviceName = "simulator"
    PlatformVersion = "26"; UDID = "simulator-id"; BundleId = "example.test"
}
$android = @{
    ServerUrl = "http://127.0.0.1:4725"; DeviceName = "emulator"
    UDID = "emulator-5554"; AppPackage = "example.test"; AppActivity = "example.Main"
}

# Inactive platform placeholders must never block the selected platform.
Assert-Missing @{ Platform = "Android"; Appium = @{ Android = $android; iOS = @{ UDID = "replace-with-id" } } } @()
Assert-Missing @{ Platform = "iOS"; Appium = @{ iOS = $ios; Android = @{ UDID = "replace-with-id" } } } @()
Assert-Missing @{ Platform = "android"; Appium = @{ Android = $android } } @()
Assert-Missing @{ Platform = "Windows" } @("Platform must be iOS or Android in local.settings.local.json")
Assert-Missing @{} @("Platform must be iOS or Android in local.settings.local.json")
Assert-Missing @{ Platform = "Android"; Appium = @{} } @(
    "Appium.Android.ServerUrl in local.settings.local.json",
    "Appium.Android.DeviceName in local.settings.local.json",
    "Appium.Android.UDID in local.settings.local.json",
    "Appium.Android.AppPackage in local.settings.local.json",
    "Appium.Android.AppActivity in local.settings.local.json"
)
$android.AppActivity = "replace-with-activity"
Assert-Missing @{ Platform = "Android"; Appium = @{ Android = $android } } @("Appium.Android.AppActivity in local.settings.local.json")
$ios.BundleId = " "
Assert-Missing @{ Platform = "iOS"; Appium = @{ iOS = $ios } } @("Appium.iOS.BundleId in local.settings.local.json")
Write-Host "Mobile local settings validation: 8 checks passed; no device/session or credentials used."
