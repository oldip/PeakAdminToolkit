# PEAK Admin Toolkit 0.8.1

Clean-room BepInEx preview. [繁體中文](README.zh-TW.md). F8 opens a resolution-scaled interface with
Overview, Items, Self tools, Players and World. The local development archive keeps completed releases in separate folders.

0.8.1 prepares a standalone GitHub source package with MIT licensing, verified
third-party description attribution and contribution/report templates. Gameplay
controls are unchanged from 0.8.0.

## Current features and authority

| Control | Who can use it | Behavior |
|---|---|---|
| Item catalog/search | Host and Client | Current-language cards and borrowed game icons; English/Chinese and Simplified/Traditional search |
| Item generation | Host and Client in a room | Native request to the Master Client; item/registration/room bans rechecked |
| God Mode | Own conscious character | Blocks new ordinary damage/negative status increments; preserves existing effects and healing |
| Infinite stamina | Own conscious character | Native no-consumption flag; positive gains capped at the current game maximum |
| Flight / no fall damage | Own conscious character | Flight keeps collision; automatic fall immunity during flight and two seconds afterward; independent fall switch |
| Cleanse | Own living character | Clear negative statuses, afflictions and thorns; no revival or stamina refill |
| Normal / no-penalty revival | Host and Client requests | Revive dead players; no-penalty choice skips new curse/hunger, retaining existing effects |
| Both teleport directions | Host and Client requests | Place conscious living players beside one another on checked ground |
| Ground-drop recovery | Host | Select candidates and move original grounded items; candidate history may include earlier discarded items |
| Time-of-day presets | Host in gameplay | 07:00, 12:00, 18:00 or 00:00; day count and run duration unchanged |
| Team warp | Host in gameplay | One click to the next-area unlit campfire or summit marker; all landing points/RPCs checked first |
| Host transfer attempt | Client | Attempt only; user tests reported failure in lobby and gameplay |

Reviving another player uses the operator as the landing anchor. Reviving
oneself uses the living spectated teammate, otherwise the last living position.
Revival may fall back in front of the teammate if no checked floor is found;
teleport requires checked ground.

World warps never light campfires or activate an ending/hidden entrance. Peak
has no supported next-area warp; 鍦版繁鍐ユ返 (Void) is the endpoint. Remote RPC
submission is not proof of the result on another machine. See TESTING.md.

Six reviewed multiplayer items can appear in normal All/use categories and be
manually generated in solo when the narrow policy permits them. Other special
variants, unused items, lobby toys and Scroll/pages need the transient Other
checkbox. Hidden chess variants remain excluded. See docs/SPECIAL_ITEMS.md.

Pinyin/initial search and character dictionaries are not embedded. Descriptions
and three interface languages are embedded; Traditional descriptions use
Windows conversion. No description/translation JSON is created or read in
config. Source language files remain separate for contributors.

## Install and settings

Close PEAK, replace the toolkit DLL in BepInEx/plugins with
out/PeakAdminToolkit.dll, and keep one toolkit/admin-menu version loaded.
Only this DLL is needed. The normal BepInEx config has three settings:

| Setting | Effect |
|---|---|
| General.Enabled | Close the window, release input, reset self tools and stop the horn HUD correction when disabled |
| Interface.ToggleKey | F8 by default; None disables the shortcut |
| Interface.Language | Auto, en, zh-CN or zh-TW; Auto follows PEAK, with English fallback |

Self tools start off. Closing F8 keeps active tools running. Scene/plugin reset
restores captured self effects. The horn HUD correction changes only the local
held slot's displayed fuel bar; it does not change item fuel.

## Build and verification

Use the local PEAK Managed directory and installed BepInEx references:

```powershell
.\tests\run.ps1 -ManagedDirectory 'D:\Steam\steamapps\common\PEAK\PEAK_Data\Managed'
.\build.cmd -ManagedDirectory 'D:\Steam\steamapps\common\PEAK\PEAK_Data\Managed' -ReferenceDirectory 'D:\Steam\steamapps\common\PEAK\BepInEx\core'
.\package.ps1
.\tests\check-release.ps1
```

The default Managed directory is ../Managed. No game assemblies are executed
by the offline tests and no PowerShell AssemblyResolve callback is used. The
Newtonsoft/netstandard CS1701 build warning is documented and non-fatal.
Source packages exclude DLL/EXE/PDB files, refs, Managed and test/build outputs;
original TSV and description hashes are preserved.

For translation-only changes, use `tests/run.ps1 -TranslationsOnly`. See
CONTRIBUTING.md, docs/CONTRIBUTING.zh-TW.md and locales/README.md.

## Compatibility and release status

Self tools, player adapters, item catalog/spawning and World controls retain
separate capability checks. Missing self/player APIs show failure details.
Safe input/cursor hooks remain a shared prerequisite for opening the UI.
Flight's automatic fall immunity depends on the fall hook; if only that hook
fails, the UI warns while flight movement remains available.

See docs/COMPATIBILITY.md for module boundaries and the repeated-work review.
There is no measured FPS claim. 1080p/1440p font rendering, Unity physics and
current-version Host/Client delivery still need live tests.

See docs/RELEASE_READINESS.md and THIRD_PARTY_NOTICES.md for the source audit.
The project is MIT licensed. Description JSON content matches the pinned upstream
revision; its MIT license and translation credit are retained separately.
See docs/GITHUB_RELEASE.md for uploading the extracted source and remaining
live regression checks. This task does not publish a repository or release.
