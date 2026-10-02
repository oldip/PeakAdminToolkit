$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$window = Get-Content -LiteralPath (Join-Path $root 'src\UI\MainWindow.cs') -Raw
$panelPath = Join-Path $root 'src\UI\WorldPanel.cs'
if (-not (Test-Path -LiteralPath $panelPath)) { throw 'World panel missing.' }
if ($window -notmatch 'localization.Text\("World"\)' -or $window -notmatch 'worldPanel\.Draw' -or $window -notmatch '\}, 5, theme\.Button') { throw 'World tab not integrated.' }
if ((Get-Content -LiteralPath $panelPath -Raw) -notmatch 'advance\.Advance\(\)') { throw 'World button does not dispatch a warp.' }
if ((Get-Content -LiteralPath $panelPath -Raw) -match 'confirmation|pendingDestination') { throw 'World still requires confirmation.' }
$keys = @('World','WorldCurrentTime','WorldCurrentDay','WorldMorning','WorldNoon','WorldEvening','WorldMidnight','WorldTimeHelp','WorldTimeChanged','WorldTimeFailed','WorldCurrentSegment','WorldNextSegment','WorldAdvance','WorldAdvanceHelp','WorldAdvanceSubmitted','WorldAdvanceFailed','WorldPeakSubmitted','WorldHostRequired','WorldGameplayRequired','WorldTimeApiUnavailable','WorldMapApiUnavailable','WorldNoNextSegment','WorldNoNextSegmentVoid','WorldPeakLandingUnavailable','WorldSegmentBeach','WorldSegmentTropics','WorldSegmentAlpine','WorldSegmentCaldera','WorldSegmentTheKiln','WorldSegmentPeak','WorldSegmentVoid')
foreach ($language in @('en','zh-CN','zh-TW')) {
    $json = Get-Content -LiteralPath (Join-Path $root "locales\$language.json") -Raw | ConvertFrom-Json
    foreach ($key in $keys) {
        if (-not $json.PSObject.Properties[$key] -or [string]::IsNullOrWhiteSpace([string]$json.$key)) { throw "Missing $language translation: $key" }
    }
}
Write-Host 'PASS: World tab, single-click warp and three locale sets.'


if ((Get-Content -LiteralPath $panelPath -Raw) -notmatch 'WorldPeakLandingUnavailable') { throw 'Peak landing failure feedback missing.' }
if ((Get-Content -LiteralPath $panelPath -Raw) -notmatch 'WorldPeakSubmitted') { throw 'Peak-specific result missing.' }
