$ErrorActionPreference = 'Stop'
$source = Get-Content -LiteralPath (Join-Path $PSScriptRoot '..\src\UI\Theme.cs') -Raw
foreach ($state in @('onHover','onFocused')) {
    if ($source -notmatch "SetButtonState\(Button\.$state, buttonSelectedHoverBackground\)") {
        throw "Selected button has no distinct hover/focus color in $state state."
    }
}
if ($source -notmatch 'buttonSelectedHoverBackground = CreateTexture\(new Color\(0\.15f, 0\.50f, 0\.45f, 1f\)\)') {
    throw 'Selected hover color is missing or not brighter than the selected normal color.'
}
Write-Host 'PASS: selected buttons visibly brighten while hovered or focused.'
