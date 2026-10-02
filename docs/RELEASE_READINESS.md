# Release readiness (reviewed 2026-10-02)

0.8.1 is a private preview with an audit of the current features. It is not a
public 1.0 release and this task does not publish it.

## Source and data

| Material | Origin | Review status |
|---|---|---|
| C# implementation | Clean-room project source | Old 1.8.2 methods are not copied; existing adapter tests retained |
| locales/*.json | Project interface translations | Separate language files, matching keys/placeholders and fallback checks |
| Three TSVs | User-identified project-owned data | Original hashes retained; not embedded in the DLL |
| Two description JSONs | User-supplied output of Peak Item Tooltip | Original hashes retained; parsed content matches pinned MIT upstream; notice included |
| Icons | PEAK runtime textures | No icon image files are packaged |
| PEAK/Unity/BepInEx/Harmony/JSON DLLs | Local build/runtime references | Excluded from source package; no dependency DLL distribution |

Description attribution is recorded in THIRD_PARTY_NOTICES.md. Parsed contents of
both supplied JSONs match upstream revision cb00e7425a89b1bae091a50fe3ebfd9684445c7f.
Original bytes are preserved. The upstream MIT notice is included. The owner
selected MIT for this project's source and translations; LICENSE is included.

Original supplied data hashes:

- English: `5E02E81045E64D33A5F5649A3D352B4014CD841841C386F223B4B997A60884CE`
- Simplified: `98B8A88F2A22D8CE362DDE5F7D6EEC0B5B1BF706057A175B71775986E2664292`
- pinyin.tsv: `67B5DEB4F8AB3D1D7FF0DE058595D293D5459AAC556EE0CEDD1D6AF52AAC7FBC`
- hans_hant.tsv: `B819643B1950D0769D224D805EE5DAA5576E49C9D519B8A33F6020EB887DED6A`
- hant_hans.tsv: `617B3C405701F6BB5A691F28A17BA1F8B66F7BF747A9A2FD1D57B85A9BEAD227`

## Live regression still needed

- 1080p and 1440p: three languages, long API failure messages, card details and
  selected/hovered button states. Geometry tests do not verify font rendering.
- Fast flight onto lower floors, flight cancellation and the two-second fall
  grace period; scene change, death/respawn and plugin disabling.
- Host whole-team warp in each supported area, especially The Kiln to Peak.
  Check both screens, reachable landing, unlit campfire and unchanged progress.
- 地深冥淵 endpoint message and single-click world action.
- Host/Client normal and no-new-penalty revival, both teleport directions and
  local-only cleanse; original ground-drop recovery and backpack contents.
- Naturally spawned friendship horn: use while held, compare the bar before
  and after dropping/picking up, with the toolkit enabled and disabled.

Prior user reports establish that item generation and local self tools can work
as Client, own cleanse works, remote cleanse does not, Host transfer attempts
failed, and basic 0.4.7 revival/teleport became usable. They do not establish
every current-version combination. Record role, game build, toolkit version,
other installed mods, recipient state and both players' observed result.

0.9.x should freeze the feature set and complete this matrix. 1.0.0 requires the
live results, compatibility checks, remaining live regressions to be closed.
