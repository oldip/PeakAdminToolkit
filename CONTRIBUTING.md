# Contributing

[繁體中文](docs/CONTRIBUTING.zh-TW.md)

## Translations

Edit the UTF-8 JSON files in `locales/`: en.json, zh-CN.json and zh-TW.json.
Translate values and preserve keys, format placeholders and newlines.
Run `tests/run.ps1 -TranslationsOnly -ManagedDirectory <PEAK_Data/Managed>`.
See locales/README.md for terminology and adding a language.

Item descriptions are embedded from `data/descriptions/`; Traditional Chinese
uses Windows conversion. Attribution is in THIRD_PARTY_NOTICES.md.

## Code

Keep feature API adapters separate and cover changed behavior with tests.
Run tests/run.ps1 and build.cmd with explicit Managed/BepInEx reference paths.
Submit a Pull Request describing the change and its verification results.

Contributions use the project MIT license.
