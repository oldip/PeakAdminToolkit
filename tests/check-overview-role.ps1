$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$window = Get-Content -LiteralPath (Join-Path $root 'src\UI\MainWindow.cs') -Raw
if ($window -notmatch 'roomRole\.Read\(\)' -or $window -notmatch 'RoomRoleHost' -or $window -notmatch 'RoomRoleClient' -or $window -notmatch 'RoomRoleOutside') {
    throw 'Overview does not display live Host, Client and outside-room states.'
}
if ($window -notmatch 'hostTransfer\.CanRequest\(\)' -or $window -notmatch 'hostTransfer\.Request\(\)') { throw 'Overview lacks guarded Client transfer attempt.' }
Write-Host 'PASS: overview displays live room role.'
