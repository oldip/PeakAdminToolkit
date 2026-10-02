$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$window = Get-Content -LiteralPath (Join-Path $root 'src\UI\MainWindow.cs') -Raw
$panel = Get-Content -LiteralPath (Join-Path $root 'src\UI\PlayerPanel.cs') -Raw -ErrorAction SilentlyContinue
$selfPanel = Get-Content -LiteralPath (Join-Path $root 'src\UI\SelfToolsPanel.cs') -Raw
if ($window -notmatch 'localization\.Text\("Players"\)' -or $window -notmatch 'playerPanel\.Draw') { throw 'Player tab is not wired into the main window.' }
foreach ($method in @('revive.Request','cleanRevive.Request','teleport.RequestTo','teleport.RequestBring')) {
    if ($panel -notmatch [regex]::Escape($method)) { throw "Player panel is missing $method." }
}
if ($panel -match 'wake\.|cleanse\.|PlayerWake|PlayerCleanse') { throw 'Wake and Cleanse must not appear in player cards.' }
if ($panel -notmatch 'if \(!self\)') { throw 'Own player card must hide teleport buttons.' }
if ($selfPanel -notmatch 'cleanse\.Request' -or $selfPanel -notmatch 'PlayerCleanse') { throw 'Own cleanse must appear under Self tools.' }
Write-Host 'PASS: player cards and Self tools expose only relevant actions.'
