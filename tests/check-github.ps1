$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
foreach ($path in @('LICENSE','README.zh-TW.md','.github\ISSUE_TEMPLATE\bug_report.md','.github\ISSUE_TEMPLATE\translation.md','.github\pull_request_template.md','docs\PEAK_ITEM_TOOLTIP_LICENSE.txt')) {
    if (-not (Test-Path -LiteralPath (Join-Path $root $path))) { throw "GitHub source file missing: $path" }
}
if ((Get-Content -LiteralPath (Join-Path $root 'LICENSE') -Raw) -notmatch 'MIT License') { throw 'MIT license missing.' }
if ((Get-Content -LiteralPath (Join-Path $root 'tests\run.ps1') -Raw) -notmatch '\[string\]\$ManagedDirectory') { throw 'Standalone tests need an explicit ManagedDirectory.' }
if ((Get-Content -LiteralPath (Join-Path $root 'tests\check-release.ps1') -Raw) -match '0\.5\.1\\data|0\.7\.2\\data') { throw 'Release checks must not depend on archived versions.' }
Write-Host 'PASS: GitHub documents, MIT notices and standalone reference/data checks.'
