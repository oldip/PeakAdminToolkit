$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$out = Join-Path $PSScriptRoot 'out\RecoveryTests.exe'
New-Item -ItemType Directory -Force -Path (Split-Path $out -Parent) | Out-Null
& $compiler /nologo /target:exe /warnaserror+ "/out:$out" (Join-Path $root 'src\Players\PlayerDirectory.cs') (Join-Path $root 'src\Players\PlayerLanding.cs') (Join-Path $root 'src\Players\DroppedItemHistory.cs') (Join-Path $root 'src\Players\DroppedItemRecovery.cs') (Join-Path $PSScriptRoot 'PlayerDoubles.cs') (Join-Path $PSScriptRoot 'RecoveryTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Recovery test compilation failed.' }
'{"runtimeOptions":{"tfm":"net8.0","rollForward":"LatestMajor","framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"}}}' | Set-Content -LiteralPath ([IO.Path]::ChangeExtension($out, '.runtimeconfig.json')) -Encoding UTF8
& dotnet $out
if ($LASTEXITCODE -ne 0) { throw 'Recovery tests failed.' }
