param([string]$CoreDirectory = 'D:\Steam\steamapps\common\PEAK\BepInEx\core')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$testOut = Join-Path $PSScriptRoot 'out\harmony'
New-Item -ItemType Directory -Force -Path $testOut | Out-Null
# Runtime dependencies come from the installed BepInEx. No PEAK/Unity assemblies are executed.
foreach ($file in (Get-ChildItem -LiteralPath $CoreDirectory -File -Filter '*.dll')) { Copy-Item -LiteralPath $file.FullName -Destination $testOut -Force }
$testExe = Join-Path $testOut 'HarmonySelfTests.exe'
$arguments = @('/nologo', '/target:exe', '/warnaserror+', '/define:REAL_HARMONY', "/out:$testExe", ('/r:' + (Join-Path $CoreDirectory '0Harmony.dll')))
$arguments += Get-ChildItem -LiteralPath (Join-Path $root 'src\Self') -Filter '*.cs' | Select-Object -ExpandProperty FullName
$arguments += Join-Path $PSScriptRoot 'SelfDoubles.cs'
$arguments += Join-Path $PSScriptRoot 'HarmonySelfTests.cs'
& $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'Harmony test compilation failed.' }
# This installed Harmony build uses Desktop CLR internals; .NET 8 AccessTools initialization is incompatible.
# The test executable catches exceptions and uses only game/Unity doubles.
& $testExe
if ($LASTEXITCODE -ne 0) { throw 'Harmony tests failed.' }
