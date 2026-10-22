function Get-MobileLocalMissingSettings {
    param([Parameter(Mandatory)]$Settings)

    $platform = [string]$Settings.Platform
    switch ($platform) {
        "iOS" {
            $platform = "iOS"
            $properties = @("ServerUrl", "DeviceName", "PlatformVersion", "UDID", "BundleId")
        }
        "Android" {
            $platform = "Android"
            $properties = @("ServerUrl", "DeviceName", "UDID", "AppPackage", "AppActivity")
        }
        default {
            "Platform must be iOS or Android in local.settings.local.json"
            return
        }
    }

    foreach ($property in $properties) {
        $value = [string]$Settings.Appium.$platform.$property
        if ([string]::IsNullOrWhiteSpace($value) -or $value -like "*replace-with*") {
            "Appium.$platform.$property in local.settings.local.json"
        }
    }
}
