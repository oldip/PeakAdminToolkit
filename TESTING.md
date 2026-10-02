# 0.8.1 testing record

## Evidence scope

The preserved 0.8.0 baseline had 1382 offline checks. The new GitHub check
first failed because LICENSE was missing. After adding the MIT license,
documentation/templates and standalone reference/data checks, the complete suite
passed 1376 counted C# checks plus seven UI/source/package checks (1383 total).
The same complete suite and Windows build passed in a fresh temporary extraction
with explicit PEAK Managed and BepInEx core paths; no archived versions were present.
The extracted project also repackaged and passed release validation.
Translation-only mode passed 14 translation and eight fallback/isolation checks.
Source ZIP validation checks labels, original hashes, notices/templates and absence
of DLL/EXE/PDB files and reference/output folders.
Tests exercise game-shaped doubles, actual installed Harmony, real embedded
JSON and Windows Chinese conversion. They do not execute PEAK assemblies or
verify Unity rendering, game physics or real Photon delivery.

On 2026-10-02, local Managed/Assembly-CSharp.dll and the installed PEAK copy both
had SHA-256 F874FA50F6E2B15D5270BF4891C1A377C22584E6F38621CB17909FFEF99923B8.
Build uses the installed PEAK Managed folder and the installed BepInEx core. The existing
Newtonsoft/netstandard CS1701 warning is non-fatal.

## Commands

- `tests/run.ps1 -ManagedDirectory <PEAK_Data/Managed>`: complete behavior, adapters, UI source checks and synthetic package-leak tests.
- `tests/run.ps1 -TranslationsOnly`: translation files, embedded runtime text and fallback checks.
- `build.cmd -ManagedDirectory <PEAK_Data/Managed> -ReferenceDirectory 'D:\Steam\steamapps\common\PEAK\BepInEx\core'`: local Windows DLL build.
- `package.ps1`, then `tests/check-release.ps1`: labels, original data hashes and binary-free source ZIP.

## New checks

- A missing fall API is named in the module's failure reason; healthy modules
  remain available without a stale error reason.
- Both revival choices share the native revive failure; missing teleport/cleanse
  members are identified independently.
- Successful type resolution is reused; cached metadata still reads the latest
  local character, ownership and consciousness. Unavailable types are retried.
- Synthetic DLL, EXE and PDB files in source input are rejected before a ZIP is
  created. Fixtures contain plain marker text, not game/dependency binaries.
- Previous flight grace-period/reset, stamina cap, Host/Client gating,
  item visibility/search, revival/teleport and World warp tests remain included.

## Manual checks and retained user reports

0.8.1 has not been manually tested in PEAK. Check new long API error text at
1080p/1440p in all three languages. Translation tests do not verify wrapping.
For a real compatibility failure, preserve the BepInEx log and confirm other
features still work. Do not claim a simulated missing API is a live game test.

Retain the earlier reports: Host/Client generation and local self tools were
usable; local cleanse worked while remote cleanse did not; Host transfer failed
in lobby and game; basic 0.4.7 revival/teleport became usable. 0.7.1 showed the
wrong endpoint message in 地深冥淵 and flight could cause fall injury.

The 0.7.2 flight immunity/grace period and whole-team World landing remain on
the live regression checklist. Verify one-click warp to unlit campfires,
The Kiln-to-Peak reachable landing, no activation of endings/hidden entrances,
and the 地深冥淵 endpoint on both Host/Client screens.

The friendship horn still needs comparison with a naturally spawned item,
with/without the toolkit. See docs/RELEASE_READINESS.md for the full remaining
matrix and source/licensing gates. Offline tests and compilation do not close
those gates.
