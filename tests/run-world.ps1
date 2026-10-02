$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$out = Join-Path $PSScriptRoot 'out\WorldTests.exe'
New-Item -ItemType Directory -Force -Path (Split-Path $out -Parent) | Out-Null
& $compiler /nologo /target:exe /warnaserror+ "/out:$out" (Join-Path $root 'src\World\WorldTime.cs') (Join-Path $root 'src\World\WorldAdvance.cs') (Join-Path $root 'src\World\WorldWarpBatch.cs') (Join-Path $PSScriptRoot 'WorldTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'World test compilation failed.' }
'{"runtimeOptions":{"tfm":"net8.0","rollForward":"LatestMajor","framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"}}}' |
    Set-Content -LiteralPath ([IO.Path]::ChangeExtension($out, '.runtimeconfig.json')) -Encoding UTF8
& dotnet $out
if ($LASTEXITCODE -ne 0) { throw 'World tests failed.' }

$out = Join-Path $PSScriptRoot 'out\WorldEndpointTests.exe'
& $compiler /nologo /target:exe /warnaserror+ "/out:$out" (Join-Path $root 'src\World\WorldVoidEndpoint.cs') (Join-Path $PSScriptRoot 'WorldEndpointTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Endpoint test compilation failed.' }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'out\WorldTests.runtimeconfig.json') -Destination ([IO.Path]::ChangeExtension($out, '.runtimeconfig.json')) -Force
& dotnet $out
if ($LASTEXITCODE -ne 0) { throw 'Endpoint tests failed.' }
