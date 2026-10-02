param([string]$CoreDirectory = 'D:\Steam\steamapps\common\PEAK\BepInEx\core')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$outDir = Join-Path $PSScriptRoot 'out\horn-harmony'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
foreach ($file in (Get-ChildItem -LiteralPath $CoreDirectory -File -Filter '*.dll')) { Copy-Item -LiteralPath $file.FullName -Destination $outDir -Force }
$out = Join-Path $outDir 'HornHudHarmonyTests.exe'
$arguments = @('/nologo','/target:exe','/warnaserror+','/define:REAL_HARMONY',"/out:$out",('/r:' + (Join-Path $CoreDirectory '0Harmony.dll')))
$arguments += Join-Path $root 'src\Items\HornFuelHud.cs'
$arguments += Join-Path $PSScriptRoot 'HornHudDoubles.cs'
$arguments += Join-Path $PSScriptRoot 'HornHudTests.cs'
& $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'Horn Harmony test compilation failed.' }
& $out
if ($LASTEXITCODE -ne 0) { throw 'Horn Harmony tests failed.' }
