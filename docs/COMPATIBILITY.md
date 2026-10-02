# Compatibility and maintenance (0.8.1)

## Boundaries

| Feature | Required API / authority | Failure behavior | Offline coverage |
|---|---|---|---|
| Window and cursor | GUIManager input/cursor getters | Window cannot open; startup warning names the shared prerequisite | Cursor lease, bounds and interface checks |
| Language | LocalizedText.CURRENT_LANGUAGE | Auto falls back to English; explicit language still works | Foundation and translation fallback tests |
| Item catalog | ItemDatabase / Item / ItemUIData | Invalid individual entries are skipped; full database failure gives an empty catalog | CatalogApiTests |
| Item spawn | Registered item, active room, native spawn API | Generation disabled; catalog remains readable | CatalogApiTests / SoloManualSpawnTests |
| God Mode | CharacterAfflictions.AddStatus | Only God Mode disabled; UI shows the failure reason | SelfToolTests / HarmonySelfTests |
| Infinite stamina | Native stamina flag and AddStamina | Only stamina module disabled; saved native flag restored | SelfToolTests / HarmonySelfTests |
| No fall damage | CharacterMovement.CheckFallDamage | Fall immunity unavailable; other tools remain usable | SelfToolTests / HarmonySelfTests |
| Flight | Movement hooks and active owned bodyparts | Flight disabled and captured gravity restored | SelfToolTests / HarmonySelfTests |
| Revival choices | RPCA_ReviveAtPosition; registered player and room | Both choices disabled with the missing API reason; teleport remains independent | PlayerTests |
| Player teleport | Character.Center / WarpPlayerRPC; conscious players | Teleport disabled with API reason; revival remains independent | PlayerTests |
| Own cleanse | CharacterAfflictions clear/remove APIs; local ownership | Cleanse disabled with API reason | PlayerTests |
| Drop recovery | Grounded original item, registered view, Host | Recovery unavailable; other player actions remain usable | RecoveryTests / CaptureTests |
| Horn fuel bar | Inventory UI and current held horn fuel | UI correction disabled and logged; fuel is never changed | HornHudTests / Harmony horn tests |
| World time | DayNightManager; Host in gameplay | Time controls disabled with a translated reason | WorldTests |
| World team warp | Map/landmark, checked standing points and player warp; Host | No request if preflight fails; time controls remain usable | WorldTests |
| Host transfer attempt | Photon master-transfer API; Client | Reports request/failure, with no local role override | RoomRoleTests |

Flight's automatic fall protection uses the NoFallDamage hook. If that hook is
unavailable but flight movement still binds, Self tools displays a warning.
The separate fall switch is not changed by flight. Its two-second grace period
ends on scene/plugin reset and never protects an old or remote character.

These boundaries do not mean every PEAK update is automatically supported.
Shared input/cursor hooks are required for safe window use. In-game physics,
network recipients and interactions with other mods require live checks.

## Repeated work review

- SelfApi caches resolved Type/FieldInfo/PropertyInfo only. It never caches a
  character, PhotonView, status value or Rigidbody. Missing types are retried.
- Local ownership/consciousness and feature eligibility are read on every
  poll/hook. A character replacement is never implicitly adopted.
- The member cache is used on Unity's main thread. No background worker was added.
- Catalog scans happen on open/refresh/language/filter changes; searching uses
  the existing catalog. Icons remain borrowed runtime textures.
- Language polling stays at one second. Theme textures/font are created once
  and destroyed on disposal. Scene/plugin callbacks restore self effects.

Tests verify reduced repeated type resolution and fresh state reads. There is
no measured claim about FPS, total allocations or game load time.

## Updating one feature

Reproduce the changed API with that module's test double, observe the failing
test, then update only its adapter/hook and translated feedback. Run its focused
test, the complete tests/run.ps1 and build.cmd against the current local Managed
files. Record the DLL hash/version used. A passing build does not verify the
remote game result. Retest affected Host/Client actions before release.
