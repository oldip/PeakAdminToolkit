$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$out = Join-Path $PSScriptRoot 'out\PlayerTests.exe'
New-Item -ItemType Directory -Force -Path (Split-Path $out -Parent) | Out-Null
$sources = @(Get-ChildItem -LiteralPath (Join-Path $root 'src\Players') -Filter '*.cs' -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName)
$sources += Join-Path $PSScriptRoot 'PlayerDoubles.cs'
$sources += Join-Path $PSScriptRoot 'PlayerTests.cs'
& $compiler /nologo /target:exe /warnaserror+ "/out:$out" @sources
if ($LASTEXITCODE -ne 0) { throw 'Player test compilation failed.' }
'{"runtimeOptions":{"tfm":"net8.0","rollForward":"LatestMajor","framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"}}}' |
    Set-Content -LiteralPath ([IO.Path]::ChangeExtension($out, '.runtimeconfig.json')) -Encoding UTF8
& dotnet $out
if ($LASTEXITCODE -ne 0) { throw 'Player tests failed.' }
