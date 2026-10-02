$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$window = Get-Content -LiteralPath (Join-Path $root 'src\UI\MainWindow.cs') -Raw
if ($window -match 'MultiplayerDiagnostics|diagnosticsPanel' -or $window -notmatch '\}, 5, theme\.Button') { throw 'Unused multiplayer diagnostics tab remains.' }
if (Test-Path -LiteralPath (Join-Path $root 'src\UI\MultiplayerDiagnosticsPanel.cs')) { throw 'Unused diagnostics panel source remains.' }
if ($window -notmatch 'roomRole\.Read' -or $window -notmatch 'playerPanel\.Draw') { throw 'Overview role and Players tab must remain.' }
Write-Host 'PASS: redundant diagnostics tab removed; Overview and Players remain.'

