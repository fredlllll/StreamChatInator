#!/usr/bin/env pwsh
<#
    Publishes StreamChatInator for one platform or all of them.

    Works on Windows, Linux and macOS with PowerShell 7+ (and on Windows with
    Windows PowerShell 5.1). Each publish also builds the React frontend
    (streamchatinatorfrontend) into the backend's wwwroot.

    Examples:
        ./publish.ps1                          # framework-dependent, all platforms
        ./publish.ps1 -Platform win-x64        # framework-dependent, Windows x64
        ./publish.ps1 -Mode self-contained     # self-contained, all platforms
        ./publish.ps1 -Platform osx-arm64 -Mode self-contained
#>
[CmdletBinding()]
param(
    [ValidateSet("all", "win-x64", "linux-x64", "osx-arm64")]
    [string]$Platform = "all",
    [ValidateSet("framework-dependent", "self-contained")]
    [string]$Mode = "framework-dependent",
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$project = Join-Path $root (Join-Path "StreamChatInator" "StreamChatInator.csproj")

$platforms = if ($Platform -eq "all") { @("win-x64", "linux-x64", "osx-arm64") } else { @($Platform) }
$prefix = if ($Mode -eq "self-contained") { "SelfContained" } else { "FrameworkDependent" }

$results = foreach ($p in $platforms) {
    $profile = "$prefix-$p"
    $outputDir = Join-Path $root "StreamChatInator\bin\Release\net10.0\$p\publish\$Mode"

    Write-Host ""
    Write-Host "==> Publishing $Mode for $p" -ForegroundColor Cyan
    if ($NoRestore) {
        dotnet publish $project "-p:PublishProfile=$profile" --no-restore
    }
    else {
        dotnet publish $project "-p:PublishProfile=$profile"
    }
    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed: $profile"
    }

    [pscustomobject]@{
        Platform = $p
        Mode     = $Mode
        Output   = $outputDir
    }
}

Write-Host ""
Write-Host "All publishes succeeded." -ForegroundColor Green
$results | Format-Table -AutoSize