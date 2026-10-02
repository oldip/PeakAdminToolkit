param([string]$BepInExCoreDirectory = 'D:\Steam\steamapps\common\PEAK\BepInEx\core', [switch]$TranslationsOnly, [string]$ManagedDirectory = (Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) 'Managed'))
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
$testOut = Join-Path $PSScriptRoot 'out'
New-Item -ItemType Directory -Force -Path $testOut | Out-Null
$jsonReference = Join-Path $ManagedDirectory 'Newtonsoft.Json.dll'
if (-not (Test-Path -LiteralPath $jsonReference)) { $jsonReference = Join-Path $root 'refs\Newtonsoft.Json.dll' }
# Compile against the SDK's 2.0 facade, not Unity's 2.1 facade.
$sdkRoot = Join-Path $env:ProgramFiles 'dotnet\sdk'
$netstandardReference = Get-ChildItem -Path (Join-Path $sdkRoot '*\Microsoft\Microsoft.NET.Build.Extensions\net461\lib\netstandard.dll') |
    Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
if (-not $netstandardReference) { throw 'Install a .NET SDK with the net461 netstandard facade to run the offline JSON tests.' }
Copy-Item -LiteralPath $jsonReference -Destination $testOut -Force
function Invoke-TestGroup([string]$Name, [string[]]$Sources) {
    $testExe = Join-Path $testOut ($Name + '.exe')
    $arguments = @('/nologo', '/target:exe', '/warnaserror+', "/out:$testExe")
    $arguments += "/r:$jsonReference", "/r:$netstandardReference"

    foreach ($language in @('en','zh-CN','zh-TW')) {
        $path = Join-Path $root ("locales\" + $language + '.json')
        if ($Name -eq 'EmbeddedFallbackTests' -and $language -eq 'zh-CN') { $path = Join-Path $root 'tests\fixtures\invalid-language.json' }
        if ($Name -eq 'EmbeddedFallbackTests' -and $language -eq 'zh-TW') { $path = Join-Path $root 'tests\fixtures\partial-language.json' }
        $arguments += "/resource:$path,PeakAdminToolkit.Locales.$language.json"
    }
    foreach ($file in Get-ChildItem -LiteralPath (Join-Path $root 'data\descriptions') -Filter '*.json' -File) {
        $path = $file.FullName
        if ($Name -eq 'EmbeddedFallbackTests' -and $file.Name.EndsWith('zh-CN.json')) { $path = Join-Path $root 'tests\fixtures\invalid-language.json' }
        $arguments += "/resource:$path,PeakAdminToolkit.Descriptions.$($file.Name)"
    }
    $arguments += Join-Path $root 'src\Core\EmbeddedJson.cs'
    foreach ($source in $Sources) { $arguments += Join-Path $root $source }
    & $compiler @arguments
    if ($LASTEXITCODE -ne 0) { throw "Test compilation failed: $Name ($LASTEXITCODE)" }
    # All groups use modern .NET; translation resources also use the game's JSON parser.
    $runtimeConfig = [IO.Path]::ChangeExtension($testExe, '.runtimeconfig.json')
    '{"runtimeOptions":{"tfm":"net8.0","rollForward":"LatestMajor","framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"}}}' |
        Set-Content -LiteralPath $runtimeConfig -Encoding UTF8
    & dotnet $testExe $root
    if ($LASTEXITCODE -ne 0) { throw "Tests failed: $Name ($LASTEXITCODE)" }
}
if ($TranslationsOnly) {
    Invoke-TestGroup 'TranslationTests' @('src\Core\Localization.cs','tests\TranslationTests.cs')
    Invoke-TestGroup 'EmbeddedFallbackTests' @('src\Core\Localization.cs','src\Items\ChineseScript.cs','src\Items\ItemDescriptions.cs','tests\EmbeddedFallbackTests.cs')
    return
}
$items = @('src\Items\SpecialItems.cs','src\UI\SpecialItemFilter.cs','src\Core\Localization.cs','src\Items\ChineseScript.cs','src\Items\ItemSearch.cs','src\Items\ItemVisibility.cs','src\Items\ItemClassification.cs')
Invoke-TestGroup 'FoundationTests' ($items + @('src\Items\ItemCatalogEntry.cs','src\UI\Toast.cs','src\UI\Tooltip.cs','src\UI\CursorLease.cs','src\UI\WindowBounds.cs','src\UI\ItemGridLayout.cs','tests\CursorDouble.cs','tests\FoundationTests.cs','tests\SpecialItemTests.cs','tests\ItemCatalogTests.cs','tests\RevisionTests.cs','tests\CardLayoutTests.cs','tests\HoverTests.cs'))
Invoke-TestGroup 'EmbeddedFallbackTests' @('src\Core\Localization.cs','src\Items\ChineseScript.cs','src\Items\ItemDescriptions.cs','tests\EmbeddedFallbackTests.cs')
Invoke-TestGroup 'DescriptionTests' ($items + @('src\Items\ItemDescriptions.cs','src\Items\ItemCatalogEntry.cs','src\UI\ItemTooltip.cs','tests\CursorDouble.cs','tests\DescriptionTests.cs'))

Invoke-TestGroup 'Assembly-CSharp' ($items + @('src\Items\ItemDescriptions.cs','src\Core\PeakApi.cs','src\Items\ItemApiAccess.cs','src\Items\ItemValidityDiagnostics.cs','src\Items\ItemCatalogApi.cs','src\Items\ItemSpawnApi.cs','src\Items\ItemManualSpawnPolicy.cs','src\Items\ItemCatalogEntry.cs','tests\CatalogApiTests.cs','tests\ItemValidityDiagnosticTests.cs','src\UI\ItemTooltip.cs','tests\MultiplayerVisibilityTests.cs','tests\SoloManualSpawnTests.cs'))

Invoke-TestGroup 'TranslationTests' @('src\Core\Localization.cs','tests\TranslationTests.cs')

& (Join-Path $PSScriptRoot 'run-self.ps1')
& (Join-Path $PSScriptRoot 'run-player.ps1')
& (Join-Path $PSScriptRoot 'run-recovery.ps1')
& (Join-Path $PSScriptRoot 'run-role.ps1')
& (Join-Path $PSScriptRoot 'check-overview-role.ps1')
& (Join-Path $PSScriptRoot 'check-player-ui.ps1')
& (Join-Path $PSScriptRoot 'check-diagnostics-ui.ps1')
& (Join-Path $PSScriptRoot 'check-theme.ps1')
& (Join-Path $PSScriptRoot 'run-harmony.ps1') -CoreDirectory $BepInExCoreDirectory

& (Join-Path $PSScriptRoot 'run-horn.ps1')

& (Join-Path $PSScriptRoot 'run-horn-harmony.ps1') -CoreDirectory $BepInExCoreDirectory
& (Join-Path $PSScriptRoot 'run-capture.ps1') -CoreDirectory $BepInExCoreDirectory

& (Join-Path $PSScriptRoot 'run-world.ps1')

& (Join-Path $PSScriptRoot 'check-world-ui.ps1')
& (Join-Path $PSScriptRoot 'check-package.ps1')

& (Join-Path $PSScriptRoot 'check-github.ps1')
