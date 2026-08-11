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
        ./publish.ps1 -SkipZip                 # don't zip each publish output

    Output goes to publish\<rid>\<mode> in the repo root, e.g.
    publish\win-x64\framework-dependent. A matching zip is written next to it,
    e.g. publish\StreamChatInator-win-x64-framework-dependent.zip.
    (publish/ is git-ignored.)
#>
[CmdletBinding()]
param(
    [ValidateSet("all", "win-x64", "linux-x64", "osx-arm64")]
    [string]$Platform = "all",
    [ValidateSet("framework-dependent", "self-contained")]
    [string]$Mode = "framework-dependent",
    [switch]$NoRestore,
    [switch]$SkipZip
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
        [Parameter(Mandatory)][string]$PublishDir,
        [switch]$SkipRestore
    )
    $extra = @("-p:PublishDir=$PublishDir")
    if ($SkipRestore) { $extra += "--no-restore" }
    & dotnet publish $Project "-p:PublishProfile=$Profile" @extra 2>&1 | ForEach-Object { Write-Host $_ }
    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed for profile '$Profile'"
    }
}

$publishRoot = Join-Path $root "publish"

$results = @()
foreach ($p in $platforms) {
    $profile = "$prefix-$p"
    $targetDir = Join-Path (Join-Path $publishRoot $p) $Mode

    Write-Host ""
    Write-Host "==> Publishing '$Mode' for '$p'" -ForegroundColor Cyan
    Write-Host "    -> $targetDir" -ForegroundColor DarkGray
    Write-Host "    (builds the React frontend too; live output below)" -ForegroundColor DarkGray

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    if (Test-Path $targetDir) {
        Write-Host "    (cleaning previous output)" -ForegroundColor DarkGray
        Remove-Item -Recurse -Force $targetDir
    }
    Invoke-StreamingDotNet -Project $project -Profile $profile -PublishDir $targetDir -SkipRestore:$NoRestore
    $sw.Stop()

    $zip = $null
    if (-not $SkipZip) {
        $zip = Join-Path $publishRoot "StreamChatInator-$p-$Mode.zip"
        Write-Host "    -> zip: $zip" -ForegroundColor DarkGray
        Compress-Archive -Path (Join-Path $targetDir "*") -DestinationPath $zip -Force
    }

    $results += [pscustomobject]@{
        Platform = $p
        Mode     = $Mode
        Seconds  = [math]::Round($sw.Elapsed.TotalSeconds, 1)
        Zip      = if ($zip) { Split-Path $zip -Leaf } else { "-" }
        Output   = $targetDir
    }
}

Write-Host ""
Write-Host "All publishes succeeded." -ForegroundColor Green
$results | Format-Table -AutoSize