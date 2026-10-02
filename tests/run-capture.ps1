param([string]$CoreDirectory = 'D:\Steam\steamapps\common\PEAK\BepInEx\core')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$outDir = Join-Path $PSScriptRoot 'out\capture'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
foreach ($file in (Get-ChildItem -LiteralPath $CoreDirectory -File -Filter '*.dll')) { Copy-Item -LiteralPath $file.FullName -Destination $outDir -Force }
$out = Join-Path $outDir 'CaptureTests.exe'
& $compiler /nologo /target:exe /warnaserror+ "/out:$out" ("/r:" + (Join-Path $CoreDirectory '0Harmony.dll')) (Join-Path $root 'src\Players\DroppedItemHistory.cs') (Join-Path $root 'src\Core\DroppedItemCapture.cs') (Join-Path $PSScriptRoot 'CaptureTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Capture test compilation failed.' }
& $out
if ($LASTEXITCODE -ne 0) { throw 'Capture tests failed.' }
