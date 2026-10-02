# 0.3.3 self tools design and API evidence

Scope: God Mode, infinite stamina, flight and fall-damage immunity for the local, owned, conscious Character only. All start off. Existing completed releases remain immutable.

## Chosen behavior
- God Mode prevents positive AddStatus for named negative statuses. Weight and explicit ignoreInvincibility calls retain game behavior. Existing statuses and healing are untouched. Scripted death and direct petrification are outside this hook.
- Infinite stamina uses the native infiniteStam property with its original value saved and restored. Positive gains are capped at GetMaxStamina; prior excess is clamped when enabled. No partial-stamina refill or injury healing.
- No fall damage skips the local CheckFallDamage method, including its fall-induced stumble. Other collision/attack damage is unchanged.
- Flight: WASD on camera horizontal plane, Space up, Ctrl down, Shift 2x. Speed slider 2–20 m/s, default 8. Keep collision. Suppress standard gravity/movement/jump/jetpack while flight controls rigidbody velocity. Menu/game input block and lost focus stop movement. Disable when climbing/carried/unconscious or ownership changes. Restore per-body gravity settings; stop flight velocity, resume native gravity immediately. Block local fall injury while flying and for two seconds after flight stops; scene/plugin reset clears the grace period.
- Scene unload/load, plugin disable/destroy and local character replacement turn all features off. Closing the menu leaves tools active.
- Each feature has its own source file, API checks and Harmony owner. Failure disables only that feature. UI translations stay in locales/*.json.

## Read-only API inspection
Source: refreshed local Managed/Assembly-CSharp.dll, SHA256 F874FA50F6E2B15D5270BF4891C1A377C22584E6F38621CB17909FFEF99923B8, matching the installed game. PEReader reads metadata/IL without executing the assembly; no AssemblyResolve. Old 1.8.2 README used only for control behavior, no method implementations copied.
- Character.localCharacter + refs.view.IsMine establish local ownership. data.fullyConscious excludes dead/downed characters.
- Character.UseStamina and OutOfStamina/OutOfRegularStamina honor infiniteStam. CharacterData.currentStamina rejects reductions while that property is true.
- Character.AddStamina calls ClampStamina, but the native flag rejects downward correction after a gain exceeds GetMaxStamina. The maximum is 0–1 with status-dependent reductions. The stamina module caps positive additions before that native call.
- CharacterAfflictions.AddStatus includes IsMine/fromRPC and ignoreInvincibility handling. statusesLocked also blocks SubtractStatus, so it is unsuitable for preserving healing.
- CharacterMovement.CheckFallDamage applies Injury and local fall/stumble. The separate hook leaves Land/ground handling intact.
- CharacterMovement.FixedUpdate updates ground/animation, obtains GetGravityForce/GetMovementForce and applies Bodypart forces. Bodypart.Rig is public. Standard movement sets per-body useGravity, so flight must override after that loop.
- CharacterSyncer reads local position/velocity for normal replication. This is static evidence only: Host/Client movement and gameplay still require live testing.

## Verification plan
Test defaults/ownership, God Mode status policy, native flag restoration, per-feature failure isolation, flight input/normalization, gravity restoration, lifecycle reset and all previous item behavior. Build using local references; package source only; verify original data and archived hashes. Live checklist covers toggles, collisions, landing, 1080p/1440p, scene changes, death/respawn and Host/Client observation.
