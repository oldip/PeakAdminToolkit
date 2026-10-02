$ErrorActionPreference = 'Stop'
$stage = Join-Path ([IO.Path]::GetTempPath()) ('PeakAdminToolkit-package-' + [Guid]::NewGuid().ToString('N'))
$packageRoot = Join-Path $stage 'PeakAdminToolkit-0.8.1-source'
$archive = Join-Path $PSScriptRoot 'PeakAdminToolkit-0.8.1-source.zip'
$files = @('README.md','CHANGELOG.md','THIRD_PARTY_NOTICES.md','CONTRIBUTING.md','LICENSE','README.zh-TW.md','build.cmd','build.ps1','package.ps1','.gitignore','.gitattributes')
try {
    New-Item -ItemType Directory -Force -Path $packageRoot | Out-Null
    foreach ($file in $files) { Copy-Item -LiteralPath (Join-Path $PSScriptRoot $file) -Destination $packageRoot }
    foreach ($directory in @('src','data','tests','locales','docs','.github')) {
        Copy-Item -LiteralPath (Join-Path $PSScriptRoot $directory) -Destination $packageRoot -Recurse -Exclude 'out'
    }
    $testOut = Join-Path $packageRoot 'tests\out'
    if (Test-Path -LiteralPath $testOut) {
        if (-not [IO.Path]::GetFullPath($testOut).StartsWith([IO.Path]::GetFullPath($packageRoot) + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid package test output path.' }
        Remove-Item -LiteralPath $testOut -Recurse -Force
    }
    foreach ($file in (Get-ChildItem -LiteralPath $packageRoot -File -Recurse)) {
        if ($file.Extension -in @('.dll','.exe','.pdb') -or $file.FullName -match '[\\/](refs|Managed|out)[\\/]') {
            throw "Source package must not contain binaries or reference/output folders: $($file.Name)"
        }
    }
    Compress-Archive -Path $packageRoot -DestinationPath $archive -CompressionLevel Optimal -Force
} finally {
    if (Test-Path -LiteralPath $stage) {
        $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
        if (-not [IO.Path]::GetFullPath($stage).StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -or (Split-Path $stage -Leaf) -notlike 'PeakAdminToolkit-package-*') { throw 'Invalid package staging path.' }
        Remove-Item -LiteralPath $stage -Recurse -Force
    }
}
Write-Host "[OK] Source archive: $archive" -ForegroundColor Green

