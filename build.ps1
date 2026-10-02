param(
    [string]$ReferenceDirectory = (Join-Path $PSScriptRoot 'refs'),
    [string]$ManagedDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) 'Managed')
)
$ErrorActionPreference = 'Stop'
$compilerCandidates = @(
    (Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'),
    (Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe')
)
$compiler = $compilerCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $compiler) { throw 'Could not find the Windows .NET Framework C# compiler (csc.exe).' }
if (-not (Test-Path -LiteralPath $ReferenceDirectory)) { throw "Reference directory not found: $ReferenceDirectory" }
$required = @('Newtonsoft.Json.dll','netstandard.dll','System.Runtime.dll','BepInEx.dll','0Harmony.dll','UnityEngine.dll','UnityEngine.CoreModule.dll','UnityEngine.PhysicsModule.dll','UnityEngine.IMGUIModule.dll','UnityEngine.InputLegacyModule.dll','UnityEngine.TextRenderingModule.dll','Zorro.Core.Runtime.dll','Assembly-CSharp.dll')
$references = @()
foreach ($name in $required) {
    $path = Join-Path $ReferenceDirectory $name
    $gamePath = Join-Path $ManagedDirectory $name
    if ($name -notin @('BepInEx.dll','0Harmony.dll') -and (Test-Path -LiteralPath $gamePath)) { $path = $gamePath }
    if (-not (Test-Path -LiteralPath $path)) { throw "Missing reference DLL: $name" }
    $references += $path
}
$outputDirectory = Join-Path $PSScriptRoot 'out'
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
$output = Join-Path $outputDirectory 'PeakAdminToolkit.dll'
Remove-Item -LiteralPath $output -Force -ErrorAction SilentlyContinue
$arguments = @('/nologo','/target:library','/optimize+','/debug:pdbonly',"/out:$output")
foreach ($path in $references) { $arguments += "/r:$path" }
foreach ($language in @('en','zh-CN','zh-TW')) {
    $path = Join-Path $PSScriptRoot ("locales\" + $language + '.json')
    $arguments += "/resource:$path,PeakAdminToolkit.Locales.$language.json"
}
foreach ($file in Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'data\descriptions') -Filter '*.json' -File) {
    $arguments += "/resource:$($file.FullName),PeakAdminToolkit.Descriptions.$($file.Name)"
}
$sources = @('src\Properties\AssemblyInfo.cs','src\Plugin.cs','src\Core\Config.cs','src\Core\Localization.cs','src\Core\EmbeddedJson.cs','src\Core\Compatibility.cs','src\Core\PeakApi.cs','src\Items\ItemApiAccess.cs','src\Items\ItemValidityDiagnostics.cs','src\Items\HornFuelHud.cs','src\Items\ItemCatalogApi.cs','src\Items\ItemSpawnApi.cs','src\Items\ItemManualSpawnPolicy.cs','src\Items\ItemCatalogEntry.cs','src\Items\ItemClassification.cs','src\Items\ChineseScript.cs','src\Items\ItemDescriptions.cs','src\Items\ItemSearch.cs','src\Items\ItemVisibility.cs','src\Items\SpecialItems.cs','src\UI\SpecialItemFilter.cs','src\UI\CursorLease.cs','src\UI\WindowBounds.cs','src\UI\ItemGridLayout.cs','src\UI\Theme.cs','src\UI\Toast.cs','src\UI\Tooltip.cs','src\UI\ItemTooltip.cs','src\UI\MainWindow.cs')
$sources += @('src\Self\SelfApi.cs','src\Self\SelfFeature.cs','src\Self\SelfTools.cs','src\Self\GodMode.cs','src\Self\InfiniteStamina.cs','src\Self\NoFallDamage.cs','src\Self\Flight.cs','src\Self\FlightMotion.cs','src\UI\SelfToolsPanel.cs')
$sources += @('src\Players\PlayerDirectory.cs','src\Players\PlayerLanding.cs','src\Players\LandingClearance.cs','src\Players\PlayerRevive.cs','src\Players\PlayerTeleport.cs','src\UI\UnityPlayerLanding.cs','src\UI\PlayerPanel.cs')
$sources += 'src\Core\RoomRoleReader.cs'
$sources += @('src\Players\PlayerNoPenaltyRevive.cs','src\Room\RoomHostTransfer.cs')
$sources += 'src\Players\PlayerCleanse.cs'
$sources += @('src\World\WorldTime.cs','src\World\WorldAdvance.cs','src\World\WorldWarpBatch.cs','src\World\WorldDestinationTeleport.cs','src\UI\WorldPanel.cs')
$sources += @('src\Players\DroppedItemHistory.cs','src\Players\DroppedItemRecovery.cs','src\Core\DroppedItemCapture.cs')
foreach ($source in $sources) {
    $path = Join-Path $PSScriptRoot $source
    if (-not (Test-Path -LiteralPath $path)) { throw "Missing source file: $source" }
    $arguments += $path
}
Write-Host '[INFO] Building PEAK Admin Toolkit 0.8.1 (embedded descriptions and interface translations; no character dictionaries or images)'
& $compiler @arguments
if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $output)) { throw "Build failed (exit $LASTEXITCODE)." }
Write-Host "[OK] Built $output" -ForegroundColor Green


