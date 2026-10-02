# Changelog

## 0.8.1

- MIT license selected by the owner; pinned upstream description content verified and MIT notice retained.
- Standalone test reference parameter and original-data hash checks remove dependence on archived releases.
- Traditional Chinese README, GitHub issue/PR templates and upload instructions included in source packaging.
- Existing gameplay behavior preserved; live regression matrix remains pending.


## 0.8.0 - compatibility feedback and source preparation

- Show missing API reasons for self tools, both revival choices, teleport and own cleanse. Preserve independent module checks and gameplay behavior.
- Cache successful self-tool type/member metadata while continuing to read current character ownership and state. Retry unavailable types.
- Add translation-only verification, terminology/contribution guidance and a compatibility/source review.
- Reject DLL, EXE and PDB leaks before source packaging. Check original TSV and description hashes.
- Retain live testing and data provenance gaps explicitly; project license selection and public publishing remain pending.
- Full offline suite: 1376 C# checks and six UI/source/package checks passed. Windows build succeeded with the existing CS1701 warning; game rendering/physics and remote results remain unverified for 0.8.0.

## 0.7.2 - terminal area, flight fall protection, one-click warp

- Mark Void (地深冥淵) as the route's endpoint with its own disabled-action message.
- Send the guarded team warp on one button click, without a confirmation step.
- Block local fall injury during flight and for two seconds after flight ends. Keep the separate No fall damage switch independent; scene/plugin reset clears the grace period.
- Offline checks and Windows build passed. In-game flight physics and Host/Client delivery still need testing.

## 0.7.1 - unlit campfire and summit warps

- Replace every native debug jump with a guarded team warp. Beach through Caldera target the current area's unlit campfire leading to the next area; players light it themselves.
- At The Kiln, use a matching unlit campfire when available, otherwise seek safe ground near PEAK's summit sequence marker. This does not depend on a spawned signal flare.
- Keep the two-click Host confirmation and preflight all player landing points and RPCs. The toolkit does not light a fire, trigger an ending or alter map progress.
- Live solo and Host/Client destination positions still need testing.
## 0.7.0 - Host world controls

- Add a separate World tab with four time-of-day presets. Do not alter day count, run duration or achievements.
- Add a two-click Host action using PEAK's own next-segment path to request whole-team advancement. Stop at Peak; never auto-jump to Void.
- Keep time and map capability checks in separate modules. Add English, Simplified Chinese and Traditional Chinese labels and offline tests.
- Multiplayer synchronization and arrival still need live verification.
## 0.5.1 - remove redundant multiplayer page

- Remove the Multiplayer diagnostics tab. Existing Players cards already show
  character state, and Overview already shows Host/Client room role.
- Remove the diagnostic-only reader, local ping sampler, translations and
  tests. Keep all 0.5.0 player and self-tool changes intact.

## 0.5.0 - read-only multiplayer diagnostics and simpler player controls

- Add a separate Multiplayer tab showing Photon room name/count, Host actor,
  each player's active state and character Owner/status, plus sampled local
  ping minimum, average and maximum. Do not infer remote-player ping.
- Remove Wake from this release. Move own-character Cleanse from Players to
  Self tools. Hide both teleport buttons on the user's own player card.
- Keep 0.4.7 revival/teleport behavior; record the user's basic live success
  while leaving 0.5.0 multiplayer diagnostics pending game verification.

## 0.4.7 - repair adjacent landing rejection

- Treat the supporting ground collider as support rather than an obstacle in
  the standing capsule check; compare Unity wrappers by object equality.
- Search a complete ring at each of two distances with a wider vertical ray.
- When no checked ground exists for revival, use the reference teammate's
  forward direction. Keep teleport guarded by checked ground.
- Write one diagnostic summary per failed landing request to the BepInEx log.
- Keep the 0.4.6 Host/Client failure report separate from unverified 0.4.7
  behavior. Revive and teleport RPCs are unchanged.

## 0.4.6 - adjacent landing and earlier hand-drop provenance

- Normal and no-new-penalty revival use checked ground beside the operator for
  another player, or beside the currently spectated living teammate for self.
  Self revival without a teammate falls back to the original position.
- Both teleport directions choose checked adjacent ground and decline the
  request if none is found.
- Capture original hand-drop PhotonViews when PEAK instantiates them, so the
  Host candidate list can survive PEAK clearing its own record on passout.
  Manual selection and ground/ownership checks remain. This path needs live
  multiplayer testing.
- Correct the plugin metadata version to 0.4.6 and record the user's 0.4.5
  Host/Client observations without treating untested 0.4.6 paths as verified.

## 0.4.5 - Client request support where the native path allows it

- Allow item generation from Clients. PEAK's `SpawnItemInHand` forwards the
  request to the Master Client; existing item validity, room and special-item
  checks remain in place.
- Allow Clients to submit normal/no-penalty revival, wake and both teleport
  RPCs for registered players. Keep state and ViewID guards and describe the
  result as a submitted request until live Host/Client verification.
- Keep remote cleanse and dropped-item recovery restricted by their distinct
  owner/provenance requirements. Update interface text in all three languages.

## 0.4.4 - selected dropped-item recovery

- Add a Host-only candidate list under each player. It reads the player's
  native dropped-item record, deduplicates live ground items, and requires the
  Host to select individual items before sending a position RPC.
- Recheck player registration, room role, living Host, item state and item
  PhotonView control when submitting. Move the original instance rather than
  spawning a copy, so backpack data is not rebuilt.
- Keep the recovery module, tests and three language strings separate. Live
  multiplayer movement and backpack preservation remain to be verified.

## 0.4.3 - separate no-penalty revival and cleanse

- Give the Host normal and no-new-penalty revival choices for any registered
  dead player. The latter uses PEAK's `applyStatus=false` path without a local
  stamina refill or extra cleanse.
- Add an independent local-owner Cleanse action for living players. It uses
  native affliction, thorn and curable-status clearing. Remote Cleanse stays
  disabled pending owner-side network verification.
- Correct player-page help in all three languages and keep the capability
  checks in separate source files. No live multiplayer outcome is claimed.

## 0.4.2 - revival choices and Host transfer attempt

- Keep normal revival with PEAK's post-revive penalty. Add a separate local
  no-penalty revival using the game's native cleanse and full-stamina calls;
  remote targets remain disabled until owner-side recovery can be verified.
- Add a Client-only Photon Master Client transfer attempt. The UI distinguishes
  request submission from an actual role change and does not unlock Host
  actions until `IsMasterClient` changes.
- Reword the six solo multiplayer cards as "does not spawn naturally in solo"
  in all three interface languages.
- Add offline regression checks for role transfer, revival choices, ownership
  and translated card labels. Live multiplayer outcomes remain unverified.

## 0.4.1 - read-only room role in Overview

- Show live Host, Client, outside-room or unavailable status in Overview,
  translated across the three interface languages.
- Keep player actions Host-gated. Do not add a forced Master Client button:
  Photon can request a transfer, but PEAK's state handoff and game behavior
  have not been verified in multiplayer.
- Add role-state and Overview wiring regressions; preserve each existing
  feature module and prior version folder.

## 0.4.0 - player management preview and selected hover feedback

- Add a player tab with registered player status, separate revive/wake actions
  and two teleport directions. Recheck room Host, target membership, status and
  PhotonView before each request. Report submission only, pending live
  Host/Client result checks.
- Selected buttons now visibly brighten on hover/focus while keeping their
  selected color distinct from ordinary buttons.
- Separate player directory, revive, wake and teleport modules with independent
  capability checks and regression tests.

## 0.3.3 - stamina maximum and selected-button hover

- Cap positive stamina gains at PEAK's current maximum while the native
  no-consumption flag is on. Clamp prior excess once when enabling; preserve
  partial stamina, original flag restoration and module isolation.
- Keep selected buttons teal during hover and keyboard focus.
- Refresh local Managed references from the installed game. Horn fuel behavior
  awaits comparison with a naturally obtained multiplayer item.

## 0.3.2 - current PEAK God Mode and horn fuel display

- Accept both six- and seven-argument `AddStatus` signatures. Current PEAK
  added `ignoreSkeleton`; its 0.3.1 BepInEx log showed the method lookup failing.
  Keep the `ignoreInvincibility` prefix argument at index five and preserve
  the existing God Mode scope. Build against installed PEAK Managed references.
- Add a separate local Bugle of Friendship HUD module. After the game redraws
  the inventory, update the selected bar from the held horn's live fuel. It
  does not mutate item data or network state and unpatches on disposal.
- Add seven-argument and legacy regression doubles, horn policy checks and
  real Harmony hook tests. 1030 offline checks and Windows build pass; in-game
  God Mode and horn behavior still require confirmation.

## 0.3.1 - God Mode startup compatibility

- Fix God Mode initialization against the current PEAK AddStatus signature.
  The 0.3.0 log showed a null `types` argument at startup because its status
  enum lookup failed; use the method's actual enum parameter instead.
- Add a regression where standalone status-enum lookup is unavailable, while
  the real method signature and Harmony hooks remain usable.
- Investigate the reported Bugle of Friendship held/inventory durability bar.
  PEAK has a fuel percentage path and solo authority path; the prefab permits
  a fuel bar. Live item data and HUD behavior still need comparison in game.
  No horn behavior is changed.
- 1004 offline checks pass; Windows build succeeds with the existing CS1701
  warning. God Mode and horn display remain unverified in PEAK.

## 0.3.0 - modular self tools

- Add local God Mode, native infinite stamina, no fall damage and collision-enabled
  flight with speed control. God Mode preserves existing injuries and healing.
- Isolate each feature and its hooks; add a separate translated Self tools page.
- Restore native flags/gravity and stop flight velocity when disabled. Scene
  changes, character replacement, death/downing and lost ownership reset tools.
- 1001 offline checks pass, including 20 using the installed Harmony library
  against API doubles. Windows build succeeds with the existing CS1701 warning.
  Live Unity physics, UI and Host/Client behavior remain unverified.

## 0.2.12 - normal solo-item browsing and unclipped status layout

- Show all six eligible solo-manual multiplayer items in All/use categories
  without requiring the Other checkbox. Keep raw validity separate and retain
  Host, registration, replacement and room-ban checks on each request.
- Keep blocked items inspectable only through Other opt-in, and preserve
  existing special-variant, unused, lobby-toy and miscellaneous opt-in rules.
- Measure localized card status text height, add vertical padding and fit its
  icon above it instead of clipping text inside a fixed 20-unit rectangle.
- 843 offline checks pass after red cases reproduced the fixed-height layout
  and hidden normal-catalog behavior. Windows build succeeds with the existing
  CS1701 warning; new font rendering and actual spawn/use await game checks.

## 0.2.11 - allow reviewed multiplayer items to be generated in solo

- Apply the user's approved solo manual-generation exception to the six
  reviewed multiplayer items, under explicit Other opt-in and Host control.
- Separate raw game loot validity from manual generation eligibility. Keep
  true-validity items in All/use categories; label solo candidates Manual spawn.
- Add ItemManualSpawnPolicy.cs. Check solo state, current LootData shape,
  name/ID room bans, registration and replacement before submitting the
  existing spawn request. Do not mutate game fields or override API errors.
- 820 offline checks pass, including 69 manual-policy checks after the first
  request test failed against 0.2.10 behavior. Windows build succeeds with the
  existing CS1701 warning; live generation/use and replication are pending.

## 0.2.10 - normal multiplayer catalog and unavailable inspection

- Show the six reviewed multiplayer items in All and their use categories when
  current game validity permits them, without requiring special opt-in.
- Opted-in Other can display measured-false multiplayer items as disabled cards
  with icons, selected-language names, descriptions and unavailable status.
- Keep API errors/unregistered/replacement entries excluded. Preserve Host and
  live validity checks; refresh is required to enable a formerly disabled card.
- Verify local banInSolo metadata and record actual 0.2.10 validity reports.
  Manual spawning is still gated; false loot validity is not proof that the
  internal instantiate path cannot create the object.
- 745 offline checks pass; Windows build succeeds with the existing CS1701
  warning. The user reports cards are visible; no new generation or remote
  replication result is claimed.

## 0.2.9 - miscellaneous scroll/pages and inherited Item candidates

- Move Scroll out of Multiplayer into Miscellaneous under the existing Other
  opt-in. Add 15 reviewed torn-page prefabs and the previously missed Fannypack.
- Correct the static audit: Guidebook and Backpack inherit Item. The allowlist
  now contains 44 candidates; existing validity/registration/replacement and
  Host gates remain unchanged. No invalid item is forced into the catalog.
- Add Miscellaneous labels and Fannypack names in separate language files.
- Preserve the actual 0.2.8 reports: 28 registered, 23 true, 5 false, 0 unknown.
  These results do not establish multiplayer generation or validity of the
  newly added candidates.
- 647 offline checks pass after the classification regression first failed.
  Windows build succeeds with the existing CS1701 warning. 0.2.9 live rendering
  and candidate generation still require in-game checks.

## 0.2.8 - read-only special-item validity diagnostics

- Add an ItemValidity report to the existing BepInEx log when loading the
  opted-in special catalog. Report actual false returns separately from
  unavailable/incompatible/throwing APIs and incomplete database scans.
- Reuse the existing validity evaluation and preserve catalog/spawn gates.
  Ordinary browsing stays quiet; failed logging cannot hide catalog entries.
- Keep report formatting in ItemValidityDiagnostics.cs, with no new config or
  embedded resources. Existing locale files carry the new version label.
- 621 offline checks pass, including 21 diagnostic regressions. Windows build
  succeeds with the existing CS1701 warning; live PEAK logs remain pending.

## 0.2.7 - catalog corrections and expanded special items

- Correct Magic Bean, Napberry, Frog Legs and umbrella-to-Glider search.
- Expand the manual Other opt-in allowlist to 28 inspected candidates,
  including Scroll, lobby props and multiplayer items; remove chess variants.
- Preserve registration, runtime validity, replacement and Host checks.
  Embed descriptions and translations; no description config is generated.
- 600 offline checks pass. Windows build succeeds with the existing CS1701
  warning. Per-item live validity and Host/Client behavior remain unverified.

## 0.2.6 - opt-in special items under Other

- Add a default-off checkbox in Other and populated special-variant, unused
  and lobby-toy subfilters. Reset on navigation and close/open; never mix
  these entries into All or normal categories.
- Admit 23 locally inspected prefab candidates through the existing runtime
  validity, registration, replacement and Host gates.
- Add translated variant/chess names and dedicated descriptions. Preserve
  exclusions for unrelated internal/prototype assets.
- Separate allowlist/localization and transient filter state into their own
  modules. 497 offline checks pass; Windows build succeeds with the existing
  CS1701 warning. DLL is 96 KiB; live special-item/Photon validation is pending.


## 0.2.5 - built-in descriptions and module boundaries

- Embed 131 English/Simplified item descriptions; convert Traditional with
  Windows. No description config generation, reading or user override.
- Split catalog and generation APIs into independent source modules; skip an
  incompatible item and guard missing APIs without disabling the basic UI.
- Move interface text into separate locales/en.json, zh-CN.json and zh-TW.json;
  add English fallback for missing/broken translations and invalid formats.
- Add translation contribution guides and source/resource validation.
- 345 offline checks passed; Windows build succeeded with the existing CS1701
  warning. DLL is 88.5 KiB. Live Unity/Photon checks remain pending.
- Test entrypoints now catch exceptions and return failures as text/exit codes,
  avoiding the Windows crash dialog triggered by an early failing test.


## 0.2.4 - external JSON descriptions and Traditional Chinese

- Load the supplied English/Simplified description JSON from BepInEx/config,
  with an optional Traditional Chinese JSON override.
- Convert missing Traditional entries via Windows and provide a converted
  131-entry Traditional file. No description table is embedded.
- Show toolkit categories/use flags with descriptions; absent text leaves
  category-only details. Preserve visibility and spawn permission rules.
- Match stable UI names with English/prefab fallbacks; reload on open/Refresh.
- All 324 offline checks pass. Build succeeds with the documented CS1701
  reference warning. In-game UI/Photon verification remains pending.

## 0.2.3 - localized item details and tooltip lifecycle

- Read PEAK's item descriptions in English, Simplified Chinese and Traditional
  Chinese; show the selected language after one second over a visible card.
- Replace global GUI tooltip tracking with explicit card/viewport hit testing.
  Card gaps, the header, scrollbar, clipped content and Overview clear hover.
- Reset tooltip timing on reopening, navigation, scrolling, filtering,
  refreshing, language changes and resizing; different items with identical
  text start their own delay.
- Size the tooltip to its wrapped content and constrain it to the window.
- Added hover hit/delay/bounds tests and description adapter tests. 303 offline
  checks and the Windows build passed; live 0.2.3 Unity/Photon checks are pending.
- Preserve the user's 0.2.2 screenshot and stale-tooltip feedback as manual
  evidence in TESTING.md. Previous release folders remain unchanged.

## 0.2.2 - compact item cards and filter corrections

- Replaced item rows with a grid of cards. Click the icon, name or card
  background to request spawning through the existing guarded API.
- Show only the selected interface language on each card; retain all stored
  translations for search and show the full selected name in its tooltip.
- Corrected Sunscreen from Recovery/Buffs to Buffs only.
- Hide consumable subfilters with no visible catalog entries, including an
  empty Revival filter; reset a removed selection to All on catalog refresh.
- Added card geometry, name-selection and populated-filter tests. All 269
  offline checks and the Windows build passed. Live 0.2.2 UI/Photon checks
  remain pending. Previous release folders are unchanged.

## 0.2.1 - icons, categories and responsive UI

- Added game-owned item icons, main category filters and consumable subfilters.
- Moved the title into the panel and scaled the entire UI for the current
  resolution; the item browser opens first and language settings stay on Overview.
- Removed pinyin/initial search and all embedded TSV resources. Windows native
  script conversion retains simplified/traditional and mixed-script search.
- Corrected duplicate detection for the Item-valued replacement reference.
- Used the supplied Managed directory for game references and added adapter
  regression tests. The DLL is 45 KiB; live 0.2.1 UI/Photon testing is pending.

## 0.2.0 - item catalog preview

- Added a PEAK item catalog using current-language item names and English labels.
- Added English, Simplified/Traditional Chinese, full pinyin and initials search.
- Omitted invalid, duplicate and known hidden/internal/unused item entries.
- Added a Host-only spawn request gated on the expected PEAK and Photon API.
- Added offline behavior checks and embedded the project-owned search tables.
- Spawn API routing is statically inspected; live Host/Client behavior remains
  to be verified in PEAK.

## 0.1.1 - readable preview window

- Replaced the transparent, scrollable panel with a high-contrast compact window.
- Kept the Close action at the bottom of the window and removed the unnecessary scrollbar.
- Kept the window inside the screen and resized it on smaller displays.
- Updated the visible version information and the source ZIP name.

## 0.1.0 - foundation

- Added the clean-room PEAK Admin Toolkit identity and standalone source tree.
- Added safe BepInEx plugin lifecycle and F8 open/close window.
- Added Auto/en/zh-CN/zh-TW UI localization with PEAK language detection.
- Added BepInEx config metadata suitable for ModConfig presentation.
- Added compatibility checks around PEAK's read-only window input/cursor API.
- Preserved the project-owned pinyin and Hans/Hant data for a later item browser.
- Deliberately left item search and gameplay actions disabled.


