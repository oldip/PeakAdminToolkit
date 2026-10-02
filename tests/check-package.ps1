$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
$fixture = Join-Path $tempRoot ('PeakAdminToolkit-package-test-' + [Guid]::NewGuid().ToString('N'))
try {
    New-Item -ItemType Directory -Path $fixture | Out-Null
    foreach ($file in @('README.md','ARCHITECTURE.md','CHANGELOG.md','TESTING.md','THIRD_PARTY_NOTICES.md','CONTRIBUTING.md','LICENSE','README.zh-TW.md','build.cmd','build.ps1','package.ps1','.gitignore','.gitattributes')) {
        Copy-Item -LiteralPath (Join-Path $root $file) -Destination $fixture
    }
    foreach ($directory in @('src','data','tests','locales','docs','.github')) {
        New-Item -ItemType Directory -Path (Join-Path $fixture $directory) | Out-Null
    }
    foreach ($extension in @('.dll','.exe','.pdb')) {
        $leak = Join-Path $fixture ('src\unwanted' + $extension)
        [IO.File]::WriteAllText($leak, 'synthetic package leak; not a binary')
        $rejected = $false
        try { & (Join-Path $fixture 'package.ps1') | Out-Null }
        catch { $rejected = $_.Exception.Message -like 'Source package must not contain*' }
        if (-not $rejected -or (Get-ChildItem -LiteralPath $fixture -File -Filter '*.zip')) { throw "Package did not reject $extension before creating an archive." }
        Remove-Item -LiteralPath $leak
    }
    Write-Host 'PASS: source packaging rejects DLL, EXE and PDB leaks before archiving.'
} finally {
    if (Test-Path -LiteralPath $fixture) {
        if (-not [IO.Path]::GetFullPath($fixture).StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -or
            (Split-Path $fixture -Leaf) -notlike 'PeakAdminToolkit-package-test-*') { throw 'Invalid package test cleanup path.' }
        Remove-Item -LiteralPath $fixture -Recurse -Force
    }
}
