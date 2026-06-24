[CmdletBinding()]
param(
    [string[]]$RevitVersions = @("2024", "2025", "2026", "2027"),
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "src\RevitAddin\SymetriFamilyForge.RevitAddin.csproj"

foreach ($version in $RevitVersions) {
    $revitInstallDir = "C:\Program Files\Autodesk\Revit $version"
    $revitApi = Join-Path $revitInstallDir "RevitAPI.dll"

    if (-not (Test-Path -LiteralPath $revitApi)) {
        throw "Revit API was not found for Revit $version at $revitApi"
    }

    Write-Host "Building Symetri Family Forge for Revit $version..."
    dotnet build $projectPath `
        --configuration $Configuration `
        --nologo `
        --verbosity:minimal `
        /clp:ErrorsOnly `
        /p:RevitVersion=$version `
        /p:RevitInstallDir="$revitInstallDir"

    if ($LASTEXITCODE -ne 0) {
        throw "Build failed for Revit $version."
    }
}

Write-Host "Build complete."
