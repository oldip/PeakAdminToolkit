$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$out = Join-Path $PSScriptRoot 'out\HornHudTests.exe'
New-Item -ItemType Directory -Force -Path (Split-Path $out -Parent) | Out-Null
$arguments = @('/nologo','/target:exe','/warnaserror+',"/out:$out")
$module = Join-Path $root 'src\Items\HornFuelHud.cs'
if (Test-Path -LiteralPath $module) { $arguments += $module }
$arguments += Join-Path $PSScriptRoot 'HornHudDoubles.cs'
$arguments += Join-Path $PSScriptRoot 'HornHudTests.cs'
& $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'Horn HUD test compilation failed.' }
'{"runtimeOptions":{"tfm":"net8.0","rollForward":"LatestMajor","framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"}}}' |
    Set-Content -LiteralPath ([IO.Path]::ChangeExtension($out, '.runtimeconfig.json')) -Encoding UTF8
& dotnet $out
if ($LASTEXITCODE -ne 0) { throw 'Horn HUD tests failed.' }
