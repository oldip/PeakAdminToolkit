# Player management

## Controls and permissions

Host and Client can submit normal/no-penalty revival and both teleport directions
through native PEAK RPCs. Revival requires a dead registered target; teleport
requires conscious living characters. The own player card omits teleport controls.
Wake is not offered. Cleanse is local-only under Self tools.

Normal revival requests native curse/hunger additions. No-penalty revival skips
those additions; neither choice guarantees full stamina or removal of every
existing status. PEAK's revive routine can retain Curse/Petrify.

Reviving another player anchors the landing beside the operator. Self revival
uses the living spectated teammate, otherwise the last living position. Nearby
ground is checked; revival can fall back in front of the teammate without a
floor guarantee. Teleport requires checked ground to avoid overlapping bodies.

## Dropped-item recovery

Host can select grounded candidates from native drop records and an isolated
DropItemRpc capture hook. This includes hand drops before death where captured.
Earlier discarded objects may appear; the candidate list does not guarantee
ownership at the time of death. Recovery moves original item instances and
rechecks ground state/authority. Capture history clears on scene transitions.

## API boundaries

- Character.AllCharacters, IsPlayerControlled and IsRegisteredToPlayer identify targets.
- CharacterData.dead and fullyPassedOut remain separate states.
- RPCA_ReviveAtPosition(position, applyStatus, segment) invokes native revival/warp.
- WarpPlayerRPC(Vector3, bool) invokes native movement.
- Requests require an active room, current target membership and a positive ViewID.
- Local status APIs require character ownership. Remote cleanse is unsupported.
- Each action binds independently; missing APIs disable the affected action.

Returning from PhotonView.RPC confirms submission only. Confirm the result on
both players' screens. Basic revival and teleport have worked in manual sessions;
the complete current-version matrix remains in TESTING.md. Host transfer attempts
failed in lobby and gameplay. No remote cleanse is offered.

API review uses PEAK metadata/IL without executing game assemblies in PowerShell.
Legacy method implementations are not copied.
