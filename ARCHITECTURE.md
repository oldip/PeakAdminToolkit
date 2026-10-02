# PEAK Admin Toolkit 0.8.1 architecture

## Module boundaries

Plugin constructs the window/input Compatibility adapter, immutable
ItemDescriptions provider and PeakApi coordinator. PeakApi delegates catalog
and generation separately to ItemCatalogApi and ItemSpawnApi. It does not
contain their reflection implementation.

ItemApiAccess contains only shared item reflection helpers. Search,
classification, visibility and Windows script conversion each stay in their
own source file. ItemTooltip handles tooltip formatting; Tooltip owns hover
timing; ItemGridLayout owns geometry. Future player/room/world tools should
have their own capability checks, API adapter and tests rather than being added
to the item modules or a monolithic PeakApi.

Catalog reading guards each item independently and the overall database read.
Generation guards its capability check and request. A changed item API can
therefore fail closed without disabling the basic window or catalog-only use.
There is no claim that all dependencies are independent: safe cursor/input
hooks remain required for opening the window.

## Self-tool isolation and restoration

0.8.1 preserves per-module failure messages for Self tools, revival, teleport
and local cleanse so the UI can identify incompatible members. SelfApi caches
successful type/member metadata on the Unity main thread; each read still uses
the current object and value. Null type resolutions are retried. No game object
or status snapshot is added by this optimization. See docs/COMPATIBILITY.md.

Plugin creates SelfTools independently of PeakApi and passes it to a separate
SelfToolsPanel. SelfApi resolves the local Character and verifies Photon IsMine
and fullyConscious. Each feature captures the target at enable and checks it
again on use; another character is never implicitly adopted. No new RPCs are
sent by these modules. Existing PEAK synchronization remains responsible for
observable player state; static inspection does not prove remote effects.

SelfFeature provides the small shared enable/disable/error boundary. GodMode,
InfiniteStamina, NoFallDamage and Flight each live in their own file. Each uses
a separate Harmony owner, distinct from window Compatibility. A partial patch
failure removes that feature's patches only. Stamina snapshots the native
infiniteStam property and caps AddStamina gains at the current game maximum.
It restores the original flag on exit.
God/fall hooks stop intercepting immediately when disabled, with no status
snapshots or HP writes. Healing continues during God Mode.

Flight preserves the normal movement/ground/animation loop and changes its
gravity/movement results and final body velocity. Native jump and jetpack use
are suppressed during flight. It snapshots only body gravity flags, never
sets kinematic/collision state, and stops flight velocity on exit. A missing or
kinematic body, climbing/carrying or lost consciousness exits flight. Unity
contact behavior and interactions with gear remain manual test requirements.
The NoFallDamage hook also consults Flight for the same local character. Flight
blocks its fall check while active and until two seconds after detach; scene or
plugin reset clears that grace period. The independent No fall damage state is
never toggled by Flight.

Plugin Update polls ownership/character identity, and resets on config disable.
OnDisable, sceneUnloaded, sceneLoaded and OnDestroy also release all effects.
Focus loss blocks flight input; closing the menu does not reset tools. All
labels/help text are separate locale JSON resources.

## Player management

DroppedItemRecovery is a separate capability. It reads CharacterItems'
`droppedItems` PhotonViews for a selected registered player and keeps only
distinct, live, Host-controlled items whose state remains Ground. The list is
not a death-only inventory: PEAK also records earlier drops. Before submitting
each selected move, it rechecks the target, room Host and item state, then
uses the original item's `SetKinematicRPC(false, position, rotation)` via
`AllViaServer`. No copy is instantiated and no backpack data is rewritten.
Remote motion and inventory persistence need a live Host/Client test.

Normal PlayerRevive, PlayerNoPenaltyRevive, PlayerCleanse and
PlayerTeleport are separate capability checks. Both revive choices use the
target view RPC with a different native penalty flag. Cleanse uses the local
owner's status APIs independently of revival; its only UI action is under Self tools.
RoomHostTransfer is separate from RoomRoleReader: requesting Host as a Client
never changes the reader's state locally. Dropped-item recovery keeps its
independent Host guard; player RPCs use a separate active-room guard.

PlayerDirectory reads only registered, player-controlled characters. It checks
active room membership and each target PhotonView before a player RPC. Revive
and Teleport have separate source files and capability checks. Each request
rechecks current character membership and state. Teleport can move the local player to another
player or bring another player to the local position. The UI reports only that
the Photon RPC call was submitted; remote results require live Host/Client
observation. See docs/PLAYER_MANAGEMENT.md for the reviewed APIs.


Overview has an independent read-only RoomRoleReader. It reads Photon
`InRoom` before `IsMasterClient` so a stale master flag outside a room cannot
be displayed as Host. A missing Photon API produces an unavailable status;
it does not change player-action permissions or other modules.

## Explicit special-item opt-in

SpecialItems holds a finite reviewed allowlist and translated labels; it does
not scan names for arbitrary unlock patterns. SpecialItemFilter owns transient
UI state. Other is the only category that permits enabling it. Category/tab
navigation and close/open reset it. There is no saved setting.

Catalog and spawn adapters accept includeSpecial for the remaining optional
specials and unavailable inspection. Multiplayer catalog visibility is based
on CanSpawnManually (raw true validity or the reviewed solo exception), so
eligible solo items enter All/use categories too. A blocked multiplayer item
appears only under Other opt-in. ItemCatalogEntry keeps raw ValidToSpawn
separate from CanSpawnManually; API errors never produce entries. Registration
and replacement exclusions apply to every path.

ItemManualSpawnPolicy handles only the six reviewed multiplayer names after a
successful false validity evaluation. It reads Photon solo state and the
prefab's LootData through GetComponent(Type), requires banInSolo, refuses
additional/alternate loot rules, and calls both RunSettings.IsItemEnabled
(string) and (ushort). Missing/changed APIs fail closed for this exception.
The policy never changes game fields or the raw validity report.

MainWindow uses CanSpawnManually plus Host capability to enable cards. Solo
manual candidates retain false ValidToSpawn for diagnostics/status, while
SpecialItemFilter uses CanSpawnManually to include them in normal All/use
categories. ItemTooltip distinguishes unavailable from solo-manual status.
ItemSpawnApi rejects disabled snapshots and rechecks raw validity and the solo
policy, without requiring the special checkbox for these six eligible items,
before invoking the existing spawn route. Refresh or reopening F8 updates snapshots; there is no per-frame
poll. Multiplayer items with true validity keep normal catalog behavior.

Scroll and a finite list of reviewed torn-page prefabs belong to Miscellaneous.
Asset review follows inheritance: Guidebook and Backpack derive from Item.
The reflection adapter already handles inherited public Item members, so no
new generation route is needed for pages or Fannypack. Unknown page names
remain excluded. Pages retain the game's selected-language names.

Normal descriptions still follow the previous data flow. Reviewed variants
use dedicated localized names/descriptions, preventing ordinary limited-use
descriptions from being attached to the infinite claw. Lobby chess colors and
variant suffixes use the selected interface language.

## Card status layout

MainWindow measures the current localized status with GUIStyle.CalcHeight.
ItemGridLayout.StatusRect reserves that height, rounded up with four units of
padding and a 28-unit minimum. IconRect fits the icon above that region with a
gap; the name starts at its existing y=92 position. Ordinary cards have no
status row and keep an 80-unit icon. The pure geometry is tested independently
of Unity font rendering, which still needs live confirmation.

## Read-only validity diagnostics

Plugin supplies a BepInEx LogInfo callback through PeakApi to ItemCatalogApi.
Only an opted-in catalog read constructs an ItemValidityDiagnostics report.
ItemApiAccess.TryIsValidToSpawn distinguishes an evaluated Boolean from an
unavailable or failing API; the existing IsValidToSpawn wrapper still fails
closed for generation. The catalog records the same evaluation before its
visibility filters, without invoking the game method a second time.

Each report counts unique recognized prefab names encountered in itemLookup.
The diagnostic module writes a summary and false/unknown details, then is
discarded. Missing database/lookup or a failed full scan yields an incomplete
report. Its logging callback is isolated so a log failure does not change
catalog results. No report writes game state, invokes spawning, adds a setting
or creates a separate file; BepInEx handles its normal log output.

## Embedded text

EmbeddedJson reads assembly resources only; it has no filesystem/config path.
ItemDescriptions loads the English and Simplified resource once. Lookup tries
stable UI name, English name and prefab name, case-insensitively. Traditional
text uses the existing Windows LCMapStringEx mapping. No pinyin or character
dictionary is embedded.

ItemCatalogApi prefers built-in descriptions, then native GetDescriptionIndex /
GetText. ItemCatalogEntry stores the three display strings. ItemTooltip combines
the selected name, toolkit category/use labels and optional text. Original JSON
type/color fields do not change classification, visibility or network authority.

Localization loads separate flat JSON resources from locales/. Missing,
blank or malformed translations fall back to English. An invalid translated
format string falls back to the English template. Unknown keys remain visible
as keys. The version label is part of the language source.

The two descriptions and three locales are public source files, embedded at
build time. There is no runtime description export, override, reload or download.
The ordinary BepInEx Enabled/ToggleKey/Language settings are unchanged.

## UI and API evidence

The card viewport, per-card hover identity and one-second delay retain prior
behavior. Navigation, filtering, scrolling, refresh, language/resolution changes
and open/close reset hover. Game textures are borrowed; no icons are packaged.

Prior read-only metadata inspection established game API shapes. The captured
0.2.3 feedback established that available native description APIs did not yield
usable text. Supplied description data fills that gap. Its gameplay values
were not independently measured.

The spawn route requires an active room and expected APIs. PEAK sends the
request to the Master Client. A submitted request
does not prove local creation or remote replication.

## Build and test runtime

PEAK supplies Newtonsoft.Json 13.0.0.0 targeting netstandard 2.0. The Unity
reference facade is 2.1, so the Windows build emits CS1701. No dependency
binary is distributed.

All offline test groups execute under modern .NET using the SDK's netstandard
2.0 compile facade. Test entrypoints catch unexpected exceptions and report
exit code 1 instead of allowing Windows crash dialogs. No PEAK assembly is
executed and no PowerShell AssemblyResolve callback is installed.

These tests verify the adapter boundaries, embedded data and pure UI logic.
Live Unity/Mono rendering and network results still need manual verification.

## World controls

WorldTime and WorldAdvance are independent reflection adapters. Both check an active gameplay scene and Photon Host membership. WorldTime calls only the native DayNightManager time setter; it never writes day count, RunManager duration or achievements. It clears the native pending-midnight flag after changing time so an early-hours morning preset cannot increment the day on the next frame. WorldAdvance limits the current segment to Beach through TheKiln and dispatches every eligible request to WorldDestinationTeleport. Peak and Void have no next-area action; Void has its own terminal message. The destination adapter seeks the current segment's unlit campfire whose advanceToSegment matches the next area. If The Kiln has no matching campfire, it tries the summit PeakSequence scene marker instead of a spawned flare. It checks nearby standing points and every player RPC, then sends WarpPlayerRPC without touching campfire state, the flare or map progress. WorldWarpBatch keeps preflight ahead of all warp requests. A missing time API disables time only; a missing map API disables advance only. WorldPanel dispatches on one click. PEAK owns multiplayer synchronization; remote effects need live tests.

