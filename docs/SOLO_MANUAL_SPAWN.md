# Solo manual generation in 0.4.5

The user approved manually generating the six reviewed multiplayer items in
solo play while retaining registration and custom-room item-ban checks. Since
0.4.5, Host and Client can submit through PEAK's Master-targeted spawn RPC.

## Eligible names

- Bugle_Magic (友情軍號)
- Cursed Skull (詛咒骷髏頭)
- ScoutEffigy (童軍雕像)
- HealingDart Variant (吹箭)
- RitualDagger (儀式匕首)
- Fannypack (腰包)

## Read-only eligibility check

The standard path uses the game's successful true IsValidToSpawn result.
Only a successful false result can reach the solo exception, which requires:

1. A name in the reviewed six-item multiplayer allowlist.
2. An active room for the actual request, and OfflineMode or one current player.
3. LootData present with banInSolo=true, excludeBasedOnCustomRunSetting=false
   and no useOtherItemForSpawningValidity reference.
4. RunSettings.IsItemEnabled(string prefabName) and IsItemEnabled(ushort itemID)
   both returning true. A missing/incompatible/throwing API blocks the exception.
5. A currently registered, non-replacement prefab, rechecked before generation.

No field is temporarily cleared, no room setting is changed, and the original
false result is still logged. The separate module contains the new policy;
API changes affecting it can disable solo manual generation while ordinary
true-validity catalog/generation remains available.

The static game inspection supporting this decision is recorded in
RUNTIME_VALIDITY_0.2.10.md. Item.IsValidToSpawn combines room settings and loot
rules. SpawnItemInHand and its RPC receiver do not directly call that method;
the receiver instantiates the named 0_Items/ prefab and calls Item.Interact.
This explains why the former broad gate blocked manual generation. It is not
proof of the resulting item interactions or client replication.

## UI and verification

In solo: open F8 > Items > All or the normal use category. Eligible cards say
Manual spawn and can be clicked by Host or Client without any special checkbox.
When raw game validity is true they stay in the normal catalog and omit this
status. Blocked candidates remain absent from normal browsing but may be
inspected as disabled cards through Other opt-in. Refresh/reopen after room
changes. Client results still need a live multiplayer check. Status text uses measured font height and
padding; the icon and name occupy separate regions.

Offline checks cover all six names, untouched game fields, no checkbox requirement,
normal/manual status and catalog membership, name/ID bans, changed loot rules, missing state,
API failures, stale entries and room/registration/replacement checks. The user
must still verify actual solo creation and each item's behavior in PEAK.
No 0.2.12 spawn or remote replication has yet been observed by this task.
