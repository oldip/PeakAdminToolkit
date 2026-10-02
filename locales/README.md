# Translation source files

One UTF-8 JSON per language; English is the canonical key set. Current supported
languages are en, zh-CN and zh-TW. Translate values only. Keep {0}/{1} parameters,
escaped newlines and literal escaped braces. Do not translate API identifiers
inserted as parameters: they identify the game member that failed.

From the repository root, run:

```powershell
.\tests\run.ps1 -TranslationsOnly -ManagedDirectory <PEAK_Data/Managed>
```

This checks matching keys, duplicate keys, nonempty values, placeholders,
embedded text and English fallback. It uses local Newtonsoft.Json plus a .NET
SDK; it does not need BepInEx/Harmony test references or a running game.

Check the UI in PEAK at 1920x1080 and 2560x1440 after editing long text. The
offline command does not exercise Unity fonts or wrapping. Translation PRs
should name the locale, include the test result and note any UI check performed.

| English term | zh-CN | zh-TW | Meaning |
|---|---|---|---|
| Host | 房主 | 房主 | Current Photon Master Client |
| Client | 客户端 | 客戶端 | Room participant other than Host |
| No-penalty revival | 无惩罚复活 | 無懲罰復活 | Skips new revival penalties; does not cleanse prior effects |
| Cleanse | 清除负面状态 | 清除負面狀態 | Own living character only |
| Warp request | 传送请求 | 傳送請求 | Submission, not proof of arrival on another machine |
| Void | 地深冥渊 | 地深冥淵 | Hidden route endpoint |

All text remains embedded at build time; no translation config is exported.
New languages require registration in Localization.cs, Config.cs, build.ps1
and tests/run.ps1. Source-mod description files remain separate under
data/descriptions and retain their original hashes.
