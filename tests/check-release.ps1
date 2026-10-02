$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
foreach ($file in @('README.md','CHANGELOG.md','TESTING.md','ARCHITECTURE.md')) {
    $text = Get-Content -LiteralPath (Join-Path $root $file) -Raw
    if ($text -notmatch '0\.8\.1') { throw "Release label missing: $file" }
}
$assembly = Get-Content -LiteralPath (Join-Path $root 'src\Properties\AssemblyInfo.cs') -Raw
if ($assembly -notmatch 'AssemblyVersion\("0\.8\.1\.0"\)' -or $assembly -notmatch 'AssemblyFileVersion\("0\.8\.1\.0"\)') { throw 'Assembly version is not 0.8.1.' }
$hashes = @{
 'data/pinyin.tsv' = '67B5DEB4F8AB3D1D7FF0DE058595D293D5459AAC556EE0CEDD1D6AF52AAC7FBC'
 'data/hans_hant.tsv' = 'B819643B1950D0769D224D805EE5DAA5576E49C9D519B8A33F6020EB887DED6A'
 'data/hant_hans.tsv' = '617B3C405701F6BB5A691F28A17BA1F8B66F7BF747A9A2FD1D57B85A9BEAD227'
 'data/descriptions/peak-item-tooltip.descriptions.json' = '5E02E81045E64D33A5F5649A3D352B4014CD841841C386F223B4B997A60884CE'
 'data/descriptions/peak-item-tooltip.descriptions.zh-CN.json' = '98B8A88F2A22D8CE362DDE5F7D6EEC0B5B1BF706057A175B71775986E2664292'
}
foreach ($path in $hashes.Keys) {
 if ((Get-FileHash -LiteralPath (Join-Path $root $path) -Algorithm SHA256).Hash -ne $hashes[$path]) { throw "Changed original data: $path" }
}
$plugin = Get-Content -LiteralPath (Join-Path $root 'src/Plugin.cs') -Raw
if ($plugin -notmatch 'Version = "0\.8\.1"') { throw 'Plugin version is not 0.8.1.' }
foreach ($language in @('en','zh-CN','zh-TW')) {
    $locale = Get-Content -LiteralPath (Join-Path $root "locales\$language.json") -Raw | ConvertFrom-Json
    if ($locale.Foundation -notmatch '0\.8\.1') { throw "UI version missing in $language." }
}$archive = Join-Path $root 'PeakAdminToolkit-0.8.1-source.zip'
if (-not (Test-Path -LiteralPath $archive)) { throw '0.8.1 source ZIP missing.' }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead($archive)
try {
    $names = @($zip.Entries | ForEach-Object { $_.FullName.Replace('\','/') })
    foreach ($path in @('LICENSE','README.zh-TW.md','.gitignore','.github/ISSUE_TEMPLATE/bug_report.md','.github/ISSUE_TEMPLATE/translation.md','.github/pull_request_template.md','docs/PEAK_ITEM_TOOLTIP_LICENSE.txt','docs/GITHUB_RELEASE.md')) {
        if ($names -notcontains "PeakAdminToolkit-0.8.1-source/$path") { throw "Missing GitHub package file: $path" }
    }
    foreach ($entry in $zip.Entries) {
        if ($entry.FullName -match '(?i)\.(dll|exe|pdb)$|/(refs|Managed|out)/') { throw "Binary/reference/output included in source ZIP: $($entry.FullName)" }
    }
} finally { $zip.Dispose() }
Write-Host 'PASS: 0.8.1 labels, original TSV/description hashes and binary-free source ZIP.'

