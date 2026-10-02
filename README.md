# PEAK Admin Toolkit 0.8.1

[繁體中文](README.zh-TW.md) · [Downloads](https://github.com/oldip/PeakAdminToolkit/releases) · [Contributing](CONTRIBUTING.md)

A BepInEx toolkit for PEAK. Press **F8** to open the resolution-scaled interface.
English, Simplified Chinese and Traditional Chinese are supported.

## Features

| Page | Controls |
|---|---|
| Overview | Current language and Host/Client status |
| Items | Item cards, game icons, categories, multilingual search and generation |
| Self tools | God Mode, infinite stamina, flight, fall protection and negative-status cleanup |
| Players | Normal/no-penalty revival, both teleport directions and selected dropped-item recovery |
| World | Time-of-day presets and whole-team travel to the next-area campfire or summit |

Host and Client can request item generation, revival and teleport. Self tools
apply to the local character. Dropped-item recovery and World controls require Host.

No-penalty revival skips new curse/hunger; it does not guarantee full stamina or
removal of existing statuses. Flight keeps collision and grants fall protection
while active and for two seconds afterward. World travel does not light campfires.
Special variants, unused items and lobby toys are available through the Other filter.

## Install

1. Install BepInEx for PEAK.
2. Download and extract `PeakAdminToolkit-0.8.1.zip` from Releases.
3. Close PEAK and place `PeakAdminToolkit.dll` in `BepInEx/plugins`.
4. Launch PEAK and press F8. Keep one toolkit version installed.

Descriptions and interface translations are embedded; only the toolkit DLL is
needed at runtime. Self tools start off. Closing the menu keeps active tools running;
disabling the plugin or changing scenes resets them.

## Settings

| Setting | Values |
|---|---|
| General.Enabled | Enable/disable the toolkit |
| Interface.ToggleKey | F8 by default; None disables the shortcut |
| Interface.Language | Auto, en, zh-CN, zh-TW; Auto follows PEAK |

## Build

Requires Windows, a .NET SDK and local PEAK/BepInEx references.

```powershell
.\tests\run.ps1 -ManagedDirectory '<PEAK>\PEAK_Data\Managed' -BepInExCoreDirectory '<PEAK>\BepInEx\core'
.\build.cmd -ManagedDirectory '<PEAK>\PEAK_Data\Managed' -ReferenceDirectory '<PEAK>\BepInEx\core'
.\package.ps1
.\tests\check-release.ps1
```

The built DLL is in `out/`. Translation sources are in `locales/`.
See [CONTRIBUTING.md](CONTRIBUTING.md) to contribute code or translations.

## License

[MIT](LICENSE). Item description credits and notices: [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
