$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$window = Get-Content -LiteralPath (Join-Path $root 'src\UI\MainWindow.cs') -Raw
if ($window -notmatch 'roomRole\.Read\(\)' -or $window -notmatch 'RoomRoleHost' -or $window -notmatch 'RoomRoleClient' -or $window -notmatch 'RoomRoleOutside') {
    throw 'Overview does not display live Host, Client and outside-room states.'
}
if ($window -match 'RoomHostTransfer|hostTransfer|RoomTransferAttempt') { throw 'Removed Host transfer action is still present.' }
Write-Host 'PASS: overview displays live room role.'
