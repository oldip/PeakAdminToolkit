# 0.2.10 observed validity and solo restriction review

Read-only review on 2026-09-29. Manual test reports indicate previously absent multiplayer
items are now visible. This is reported visibility, not a screenshot or
proof of generation success.

The installed log identifies 0.2.10. Earlier loads in this log report 44
registered candidates, 38 true, 6 false and 0 unknown. A later report has
44 true and 0 false. The diagnostic does not capture room role, player count
or scene, so the reason for this transition is not established by the log.

Source: `D:/Steam/steamapps/common/PEAK/BepInEx/LogOutput.log`.
File modification time (not individual event time): `2026-09-29T00:44:14.885956+08:00`.
Source SHA-256: `970231A5A61E517002200638969BEF7050E023F25347C9472A398AAF959B22BD`.

```text
[Info   :   BepInEx] Loading [PEAK Admin Toolkit 0.2.10]
[Info   :PEAK Admin Toolkit] [ItemValidity] status=complete registered=44 true=38 false=6 unknown=0
[Info   :PEAK Admin Toolkit] [ItemValidity] false: Bugle_Magic
[Info   :PEAK Admin Toolkit] [ItemValidity] false: Cursed Skull
[Info   :PEAK Admin Toolkit] [ItemValidity] false: ScoutEffigy
[Info   :PEAK Admin Toolkit] [ItemValidity] false: HealingDart Variant
[Info   :PEAK Admin Toolkit] [ItemValidity] false: Fannypack
[Info   :PEAK Admin Toolkit] [ItemValidity] false: RitualDagger
[Info   :PEAK Admin Toolkit] [ItemValidity] status=complete registered=44 true=38 false=6 unknown=0
[Info   :PEAK Admin Toolkit] [ItemValidity] false: Bugle_Magic
[Info   :PEAK Admin Toolkit] [ItemValidity] false: Cursed Skull
[Info   :PEAK Admin Toolkit] [ItemValidity] false: ScoutEffigy
[Info   :PEAK Admin Toolkit] [ItemValidity] false: HealingDart Variant
[Info   :PEAK Admin Toolkit] [ItemValidity] false: Fannypack
[Info   :PEAK Admin Toolkit] [ItemValidity] false: RitualDagger
[Info   :PEAK Admin Toolkit] [ItemValidity] status=complete registered=44 true=44 false=0 unknown=0
```

## Static local game inspection

All six prefabs have LootData.banInSolo = 1:
Bugle_Magic, Cursed Skull, ScoutEffigy, HealingDart Variant, RitualDagger,
and Fannypack. Their excludeBasedOnCustomRunSetting is 0 and
useOtherItemForSpawningValidity is empty in the inspected resources.assets.

PEReader inspection shows LootData.IsValidToSpawn reads banInSolo,
PhotonNetwork.OfflineMode, InRoom and CurrentRoom.PlayerCount. Item.IsValidToSpawn
also consults RunSettings and LootData. This supports the observed behavior
of the restriction in a one-player session.

SpawnItemInHand sends RPC_SpawnItemInHandMaster. That receiver calls
PhotonNetwork.Instantiate with the 0_Items/ prefab path, then Item.Interact.
Neither inspected method directly calls IsValidToSpawn. Thus a false loot
validity result alone does not prove the manual spawn path cannot instantiate
or use the prefab. It also does not prove every prefab works in solo play.

0.2.10 deliberately retains the prior generation gate. Unavailable cards are
inspectable only; a separate change would be needed to allow manual generation
when only the solo loot restriction applies. No legacy method body was read
or copied, and no game method was executed by these inspection tools.
