# Observed 0.2.8 validity reports

The user loaded 0.2.8 and enabled diagnostics. Read-only log review found two
complete reports with the same results: 28 registered candidates, 23 true,
5 false, 0 unknown. These are real API returns, unlike offline test fixtures.

Source: `D:/Steam/steamapps/common/PEAK/BepInEx/LogOutput.log`.
File modification time: `2026-09-28T23:49:01.716328+08:00` (file timestamp, not individual event time).
Source SHA-256 at capture: `6226DC8AE61F896EEDDC3E8211EA789C7C4AC15A63F9488C1FAF424E5D20183C`.
The source log also identifies PEAK Admin Toolkit 0.2.8 and Window hooks: True.
Room role, participant count and scene were not captured by the diagnostic;
do not label this as a verified solo, Host or Client result.

| Prefab returning false | Traditional Chinese label |
|---|---|
| Bugle_Magic | 友情軍號 |
| Cursed Skull | 詛咒骷髏頭 |
| ScoutEffigy | 童軍雕像 |
| HealingDart Variant | 吹箭 |
| RitualDagger | 儀式匕首 |

The same items can have different results under different game settings.
This does not establish generation success, usability or remote replication.
The 15 page prefabs and Fannypack added to 0.2.9 were outside the 0.2.8
allowlist, so this report contains no validity result for those additions.

Relevant log lines only:

```text
[Info   :PEAK Admin Toolkit] [ItemValidity] status=complete registered=28 true=23 false=5 unknown=0
[Info   :PEAK Admin Toolkit] [ItemValidity] false: Bugle_Magic
[Info   :PEAK Admin Toolkit] [ItemValidity] false: Cursed Skull
[Info   :PEAK Admin Toolkit] [ItemValidity] false: ScoutEffigy
[Info   :PEAK Admin Toolkit] [ItemValidity] false: HealingDart Variant
[Info   :PEAK Admin Toolkit] [ItemValidity] false: RitualDagger
[Info   :PEAK Admin Toolkit] [ItemValidity] status=complete registered=28 true=23 false=5 unknown=0
[Info   :PEAK Admin Toolkit] [ItemValidity] false: Bugle_Magic
[Info   :PEAK Admin Toolkit] [ItemValidity] false: Cursed Skull
[Info   :PEAK Admin Toolkit] [ItemValidity] false: ScoutEffigy
[Info   :PEAK Admin Toolkit] [ItemValidity] false: HealingDart Variant
[Info   :PEAK Admin Toolkit] [ItemValidity] false: RitualDagger
```
