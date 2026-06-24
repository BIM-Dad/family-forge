[CmdletBinding()]
param(
    [string]$RevitVersion = "2026",
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "src\RevitAddin\SymetriFamilyForge.RevitAddin.csproj"
$addinTemplate = Join-Path $repoRoot "src\RevitAddin\SymetriFamilyForge.addin.template"
$revitInstallDir = "C:\Program Files\Autodesk\Revit $RevitVersion"
$revitApi = Join-Path $revitInstallDir "RevitAPI.dll"
$addinTargetDir = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$RevitVersion"
$addinTarget = Join-Path $addinTargetDir "SymetriFamilyForge.addin"
$builtDll = Join-Path $repoRoot "src\RevitAddin\bin\Revit$RevitVersion\SymetriFamilyForge.RevitAddin.dll"

Write-Host "Installing Symetri Family Forge Revit add-in"
Write-Host "Repository root: $repoRoot"
Write-Host "Revit version:   $RevitVersion"
Write-Host "Revit API:       $revitApi"
Write-Host ""

if (-not (Test-Path -LiteralPath $revitApi)) {
    [Console]::Error.WriteLine("Install failed: Revit API was not found at '$revitApi'.")
    exit 1
}

dotnet build $projectPath `
    --configuration $Configuration `
    --nologo `
    --verbosity:minimal `
    /clp:ErrorsOnly `
    /p:RevitVersion=$RevitVersion `
    /p:RevitInstallDir="$revitInstallDir"

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

if (-not (Test-Path -LiteralPath $builtDll)) {
    [Console]::Error.WriteLine("Install failed: built add-in DLL was not found at '$builtDll'.")
    exit 1
}

if (-not (Test-Path -LiteralPath $addinTemplate)) {
    [Console]::Error.WriteLine("Install failed: add-in template was not found at '$addinTemplate'.")
    exit 1
}

New-Item -ItemType Directory -Force -Path $addinTargetDir | Out-Null

$assemblyPath = (Resolve-Path -LiteralPath $builtDll).Path
$manifestText = Get-Content -LiteralPath $addinTemplate -Raw
$manifestText = $manifestText.Replace("{ASSEMBLY_PATH}", $assemblyPath)

try {
    Set-Content -LiteralPath $addinTarget -Value $manifestText -Encoding UTF8
}
catch {
    [Console]::Error.WriteLine("Install failed: could not write '$addinTarget'. Close Revit $RevitVersion and try again.")
    [Console]::Error.WriteLine($_.Exception.Message)
    exit 1
}

Write-Host ""
Write-Host "Installed manifest:"
Write-Host $addinTarget
Write-Host ""
Write-Host "Assembly path:"
Write-Host $assemblyPath
Write-Host ""
Write-Host "Restart Revit $RevitVersion after installing, then run External Tools > Symetri Family Forge - Build Family From Recipe."

