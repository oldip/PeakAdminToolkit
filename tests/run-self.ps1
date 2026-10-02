$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$testOut = Join-Path $PSScriptRoot 'out'
New-Item -ItemType Directory -Force -Path $testOut | Out-Null
$testExe = Join-Path $testOut 'SelfToolTests.exe'
$arguments = @('/nologo', '/target:exe', '/warnaserror+', "/out:$testExe")
$arguments += Get-ChildItem -LiteralPath (Join-Path $root 'src\Self') -Filter '*.cs' | Select-Object -ExpandProperty FullName
$arguments += Join-Path $PSScriptRoot 'SelfDoubles.cs'
$arguments += Join-Path $PSScriptRoot 'SelfToolTests.cs'
& $compiler @arguments
if ($LASTEXITCODE -ne 0) { throw 'Self tool test compilation failed.' }
'{"runtimeOptions":{"tfm":"net8.0","rollForward":"LatestMajor","framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"}}}' |
    Set-Content -LiteralPath ([IO.Path]::ChangeExtension($testExe, '.runtimeconfig.json')) -Encoding UTF8
& dotnet $testExe
if ($LASTEXITCODE -ne 0) { throw 'Self tool tests failed.' }
