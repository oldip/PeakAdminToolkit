# Third-party notices

## Preserved description data

The English and Simplified JSON files in data/descriptions/ were supplied from another
mod's BepInEx/config output. Original bytes are preserved:

- English SHA-256: 5E02E81045E64D33A5F5649A3D352B4014CD841841C386F223B4B997A60884CE
- Simplified SHA-256: 98B8A88F2A22D8CE362DDE5F7D6EEC0B5B1BF706057A175B71775986E2664292

Both files are embedded in the DLL; Traditional descriptions are converted at
runtime by Windows. No source-mod method was copied.

On 2026-10-02, both supplied JSONs were compared with
[knekly/peak-item-tooltip](https://github.com/knekly/peak-item-tooltip) at revision
`cb00e7425a89b1bae091a50fe3ebfd9684445c7f`:

- `assets/default-descriptions.json`: SHA-256 `1D4560EFEF1A938D55B618CB87EEA32EF188DDDDAE04B5E5699D2C0073F86C30`
- `assets/default-descriptions-zh-CN.json`: SHA-256 `F12CB5657F1719C40E61B2B8070CAE36900CD5AAF57F42FEF9A9D18EA925AC5B`

Parsed JSON content is identical for each file. Raw bytes/hashes differ; the
supplied originals remain unchanged. Upstream is MIT, copyright (c) 2026 katcw;
its README credits Gintoki000 for Simplified Chinese. The complete upstream
notice is retained in `docs/PEAK_ITEM_TOOLTIP_LICENSE.txt`. No upstream C#
method bodies were read or copied for this attribution check.

## Local dependencies

Build uses locally installed PEAK, BepInEx, Harmony, Unity and Newtonsoft.Json
references. No dependency DLL is redistributed. Runtime uses the game's
existing Newtonsoft.Json 13.0.0.0.

Game icons are borrowed runtime textures. Windows provides Chinese conversion;
no language library or character table is embedded. Interface translations
are separate project source files in locales/ and are embedded at build time. Original project-owned TSVs
are retained for provenance. Old 1.8.2 supplies category/behavior data without
copying its methods.

Project-owned source and interface translations are MIT licensed under LICENSE.
Game and dependency ownership is not transferred by this license.
