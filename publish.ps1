#!/usr/bin/env pwsh
<#
    Publishes StreamChatInator for one platform or all of them.

    Works on Windows, Linux and macOS (PowerShell 7+) and on Windows with
    Windows PowerShell 5.1. Output streams live to the console as the build
    progresses; a nonzero exit code stops the script immediately.

    Examples:
        ./publish.ps1                          # framework-dependent, all platforms
        ./publish.ps1 -Platform win-x64        # framework-dependent, Windows x64
        ./publish.ps1 -Mode self-contained     # self-contained, all platforms
        ./publish.ps1 -Platform osx-arm64 -Mode self-contained
        ./publish.ps1 -NoRestore               # reuse existing restore results
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
# Native stderr (e.g. MSBuild lines that land on stderr on some PS versions) must
# not abort the script.
$PSNativeCommandUseErrorActionPreference = $false

$root = $PSScriptRoot
$project = Join-Path $root (Join-Path "StreamChatInator" "StreamChatInator.csproj")

$platforms = if ($Platform -eq "all") { @("win-x64", "linux-x64", "osx-arm64") } else { @($Platform) }
$prefix = if ($Mode -eq "self-contained") { "SelfContained" } else { "FrameworkDependent" }

# Runs dotnet publish and forwards every line to the console as it is produced,
# so the script never goes silent while the build is working.
function Invoke-StreamingDotNet {
    param(
        [Parameter(Mandatory)][string]$Project,
        [Parameter(Mandatory)][string]$Profile,
        [switch]$SkipRestore
    )
    if ($SkipRestore) {
        & dotnet publish $Project "-p:PublishProfile=$Profile" --no-restore 2>&1 | ForEach-Object { Write-Host $_ }
    }
    else {
        & dotnet publish $Project "-p:PublishProfile=$Profile" 2>&1 | ForEach-Object { Write-Host $_ }
    }
    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed for profile '$Profile'"
    }
}

$results = @()
foreach ($p in $platforms) {
    $profile = "$prefix-$p"
    $outputDir = Join-Path $root "StreamChatInator\bin\Release\net10.0\$p\publish\$Mode"

    Write-Host ""
    Write-Host "==> Publishing '$Mode' for '$p'" -ForegroundColor Cyan
    Write-Host "    (builds the React frontend too; live output below)" -ForegroundColor DarkGray

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    Invoke-StreamingDotNet -Project $project -Profile $profile -SkipRestore:$NoRestore
    $sw.Stop()

    $results += [pscustomobject]@{
        Platform = $p
        Mode     = $Mode
        Seconds  = [math]::Round($sw.Elapsed.TotalSeconds, 1)
        Output   = $outputDir
    }
}

Write-Host ""
Write-Host "All publishes succeeded." -ForegroundColor Green
$results | Format-Table -AutoSize