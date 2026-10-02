# Contributing

This preview is prepared for future source publication. No repository has been
published automatically. Traditional Chinese guide: docs/CONTRIBUTING.zh-TW.md.

## Interface translations

Edit one UTF-8 JSON file per language in locales/:

- en.json: English and canonical message keys.
- zh-CN.json: Simplified Chinese.
- zh-TW.json: Traditional Chinese.

Translate values only. Preserve keys, {0}/{1} format parameters and intentional
newlines. Do not add duplicate keys or empty values. Update each language's
Foundation value when preparing a new version.

Run `tests/run.ps1 -TranslationsOnly` for a translation-only contribution.
Run tests/run.ps1 for a code or release change. TranslationTests checks key sets, nonempty values, duplicate
keys, format parameters and actual embedded/runtime text. Runtime fallback tests
also cover missing keys, malformed JSON and bad format strings.

Improving supported translations requires no C# changes. For a new language,
add a locale file and wire its code/game enum in Localization.Resolve, the
language preference values in Config, the selector/name in MainWindow and the
resource lists in build.ps1/tests/run.ps1. Add tests for the new mapping. This
registration is explicit in the current three-language preview. See
locales/README.md for terminology and the two-resolution UI checklist.

## Item descriptions

data/descriptions/ contains the unchanged user-supplied source datasets.
Original hashes and unresolved attribution are in THIRD_PARTY_NOTICES.md.
Traditional item descriptions currently use Windows conversion; do not add a
duplicate conversion dictionary. Propose translation corrections separately
with their source/provenance instead of silently replacing the archived originals.

Language source files are embedded during compilation. They are not runtime
config files and are not written into the player's config directory.

Special-item names/descriptions and subtype labels are also in locales/.
Adding a special prefab requires independent asset/API evidence, an explicit
allowlist entry in SpecialItems.cs and tests; do not broaden hidden-name
patterns. See docs/SPECIAL_ITEMS.md.

## Code changes

Keep each feature's API adapter separate. Add tests for missing/changed APIs
before modifying that feature. A failing generation API must not disable the
catalog; a failing item must not hide unrelated items. Shared input hooks remain
necessary for safe UI operation. Keep clean-room restrictions: do not copy old
1.8.2 method implementations or redistribute game/dependency DLLs.

In the local version archive, completed release directories are immutable.
In the standalone GitHub repository, work on a branch and preserve published
versions with tags. Run tests/run.ps1 and build.cmd, then package.ps1. Check the
ZIP excludes DLL/EXE/PDB files, refs/, Managed/ and out/. Preserve original TSV
hashes and distinguish offline tests from manual Unity/Photon evidence.

When the GitHub repository is published, submit a pull request from a fork or
branch with the language/feature scope and verification results. Retain the MIT license and third-party notices in redistributed copies.

## License and references

Contributions are provided under the project MIT license. Do not submit game DLLs
or copied legacy mod methods. Use explicit `-ManagedDirectory` and
`-BepInExCoreDirectory` with tests/run.ps1 when building an extracted source tree.
Translations live in locales/; preserve JSON keys and placeholders.
