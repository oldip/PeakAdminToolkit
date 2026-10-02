# Special-item review for 0.2.12

The 0.2.9 inventory has 44 explicit candidates. Scroll moves to Miscellaneous;
15 torn-page prefabs and Fannypack are added after inspecting Item inheritance.
0.2.11 adds the approved solo manual-generation exception for the six reviewed
multiplayer items. In 0.2.12 eligible solo items appear in the normal catalog
without opt-in; the raw validity diagnostic remains unchanged. In 0.4.5,
Host and Client can submit the native Master-targeted spawn request; room,
registration, replacement and room item-ban checks still apply.

Read-only PEAK asset and DLL metadata inspection on 2026-09-28. The supplied
Managed assembly matches the installed game's Assembly-CSharp.dll:
`C067125B9833EF1F2881AB97FDCA574BE7C67F45C8ADD77B9E0A8AB82C4FE9F7`.

`resources.assets` SHA-256:
`84E5A7C54567BFD9BF3280A00B4D01A3AA1D004B8018006661BF401E42E1BA39`.

## Explicit opt-in candidates

Multiplayer items eligible for either normal or solo-manual generation appear
in All/use categories. Blocked multiplayer entries and all other special kinds
require the Other opt-in. The list is a finite prefab allowlist; no broad
`C_`, `_UNUSED`, `_PROP` or `_TEMP` rule is enabled.

| Prefab | Item ID | Group |
|---|---:|---|
| RescueHook_Infinite | 167 | Special variants |
| ScoutCookies_Vanilla | 157 | Special variants |
| Parachute | 176 | Unused items |
| C_Bishop B / C_Bishop W | 127 / 128 | Lobby toys |
| C_King B / C_King W | 129 / 130 | Lobby toys |
| C_Knight B / C_Knight W | 131 / 132 | Lobby toys |
| C_Pawn B / C_Pawn W | 133 / 134 | Lobby toys |
| C_Queen B / C_Queen W | 135 / 136 | Lobby toys |
| C_Rook B / C_Rook W | 137 / 138 | Lobby toys |
| Basketball / Warpsketball | 31 / 175 | Lobby toys |
| Binoculars_Prop | 125 | Lobby toys |
| Lollipop_Prop | 139 | Lobby toys |
| Bugle_Prop Variant | 126 | Lobby toys |
| BingBong_Prop Variant | 124 | Lobby toys |
| Passport | 59 | Lobby toys |
| GuidebookPageScroll Variant | 49 | Miscellaneous |
| Cursed Skull | 25 | Multiplayer items |
| Bugle_Magic | 16 | Multiplayer items |
| ScoutEffigy | 67 | Multiplayer items |
| HealingDart Variant | 70 | Multiplayer items |
| RitualDagger | 173 | Multiplayer items |
| GuidebookPage_10_Sleepy | 86 | Miscellaneous |
| GuidebookPage_0_Intro | 50 | Miscellaneous |
| GuidebookPage_11_Awake | 87 | Miscellaneous |
| GuidebookPage | 80 | Miscellaneous |
| Fannypack | 166 | Multiplayer items |
| GuidebookPage_2_Campfire | 52 | Miscellaneous |
| GuidebookPage_6_BodyHeat | 85 | Miscellaneous |
| GuidebookPage_7_BurningSun | 188 | Miscellaneous |
| GuidebookPage_9_Gloom | 190 | Miscellaneous |
| GuidebookPage_8_Magma | 189 | Miscellaneous |
| GuidebookPage_1_Mushrooms | 82 | Miscellaneous |
| GuidebookPage_5_Zombies | 187 | Miscellaneous |
| GuidebookPage_3_Revival | 53 | Miscellaneous |
| GuidebookPage_4_Poison | 186 | Miscellaneous |
| GuidebookPage_12_Crashout | 54 | Miscellaneous |
| GuidebookPage_13_FirstTeams | 96 | Miscellaneous |

This is 44 candidates: two special variants, one unused item, 19 lobby toys,
six multiplayer items and 16 miscellaneous items. The 12 regular black/white chess pieces remain;
the six chess variants are excluded.

## Asset evidence and limits

PEReader inspected `Item.IsValidToSpawn()` and the existing
`SpawnItemInHand(string)` route without loading or executing game code.
`IsValidToSpawn()` consults runtime `RunSettings` and `LootData`; static
inspection does not establish its result for a specific item in a live run.

The serialized prefab records for the candidates have `Item` or an Item-derived component,
item ID and empty replacement reference. This does not prove that the current
runtime database registers each one, that `IsValidToSpawn()` returns true, or
that a spawn succeeds or replicates to clients. Two collected 0.2.8 reports include five actual false results; see
RUNTIME_VALIDITY_0.2.8.md. Offline adapter doubles cannot establish actual
validity of a specific live candidate. Raw game validity is recorded unchanged;
manual spawning follows the narrowly scoped solo policy in SOLO_MANUAL_SPAWN.md.

The earlier scan missed subclasses by checking only the literal component name
`Item`. Metadata inspection confirms `Guidebook : Item` and `Backpack : Item`.
Fannypack has a Backpack component with item ID 166. The 15 page prefabs have
Guidebook components, distinct item IDs and empty replacement references.
They expose the inherited Item fields/methods to the existing adapter. This
corrects the earlier Fannypack assessment; no new spawn API or bypass is used.

Manual test reports indicate Scroll/pages are also present in solo play. They are grouped
as Miscellaneous for browsing, without inferring any particular runtime
validity result. Page names remain supplied by PEAK localization; the embedded
Scroll/Torn Page descriptions already explain their relationship.

UnityPy 1.25.3 and TypeTreeGeneratorAPI 0.0.10 were local inspection tools
only. No game assets or inspection tools are runtime dependencies or included
in the source ZIP.

## UI and test behavior

Known multiplayer entries eligible for normal or reviewed solo-manual
generation appear in All/use categories without opt-in. The latter keep raw
false validity and a Manual spawn status. The checkbox is still required to
inspect multiplayer entries blocked by other rules, which remain disabled.
Unknown API results, unregistered/replacement entries and invalid non-multiplayer
specials remain excluded.

The checkbox resets on navigation/window close and is not persisted. Eligible
multiplayer entries remain in the normal catalog across these actions. Refresh
or reopening F8 updates raw validity and manual eligibility. Each request
rechecks registration, replacement, authority and the current game/policy result.
See SOLO_MANUAL_SPAWN.md. Other special kinds retain the existing requirements.

Offline tests verify the 44-name allowlist, excluded chess variants, grouping,
localized labels, description fallback, manual opt-in and rejection through
the adapter gates. They do not prove live item validity, creation, interaction,
durability or remote replication. See `../TESTING.md` for the remaining PEAK
checks.

## Collecting actual validity results

In 0.2.8, enable special items in Other, then inspect `[ItemValidity]` lines in
`BepInEx/LogOutput.log`. Refresh while enabled to get a fresh report. A `false:`
line records an actual false return; `unknown:` records an unavailable or
failed evaluation. The summary counts only recognized candidate names found
in the runtime lookup. `status=incomplete` warns that the scan did not finish
or the database was unavailable. Logs are separate from generation requests
and reuse the catalog's existing validity calls.

The 0.2.8 reports are preserved in RUNTIME_VALIDITY_0.2.8.md. They cover the old
28-candidate allowlist only. The newer 0.2.10 reports cover all 44 candidates; see RUNTIME_VALIDITY_0.2.10.md.
The diagnostic remains read-only and does not submit spawn requests.
