# Player-management API review through 0.5.1

## 0.5.0 interface scope

The user confirmed basic 0.4.7 revival and teleport worked in PEAK. In
0.5.0, Wake is removed from the build and player UI. Cleanse is available
only for the local owned living character under Self tools. The own player
card omits both teleport controls; other player cards retain both directions.
Earlier Wake references below describe archived releases only.

## 0.4.7 live failure follow-up

The user reported that both Host and Client received a request-failed message
for revival and teleport in 0.4.6. Those actions share the new adjacent
landing check; the installed BepInEx log showed no RPC error. The standing
capsule could overlap its supporting floor and reject every candidate. The
0.4.7 check ignores that same collider, expands the search and logs rejection
counts on failure. This is a static and offline-tested cause, not yet confirmed
by a 0.4.7 live result.

If revival cannot locate checked ground, the user chose a front-of-teammate
fallback. This uses the teammate's horizontal facing direction without a floor
guarantee; teleport continues to require a checked landing.

## 0.4.6 landing and hand-drop changes

PEAK's original warp RPC accepts a target center. Passing another character's
exact center caused overlapping bodies in the user's live test. The new
`UnityPlayerLanding` checks nearby terrain/map ground, slope and a standing
capsule before either teleport or near-player revival sends its existing RPC.
The result remains subject to live collision and network validation.

The original `droppedItems` list is cleared on passout. PEAK's hand-drop path
can instantiate a room item without retaining it in that list. A separate
Harmony hook records the returned PhotonView with the owning character during
`DropItemRpc`; the recovery list unions that history with native entries and
rechecks ground state and Host ownership. History clears on scene transitions.
The hook is isolated from revival, teleport and native backpack recovery.

The user confirmed that a Client cannot take over Host in lobby or game;
Host remote cleanse did not work. Static inspection shows the full cleanse
path is character-owner local, so the remote button remains disabled. The user
reported that Client item spawn, revival, wake and self tools worked in their
session; exact remote outcomes after 0.4.6 landing changes remain pending.

## 0.4.5 Client request boundary

The installed PhotonView RPC sender and the reviewed PEAK revival, wake and
warp handlers have no Host-only check. The toolkit now allows Host and Client
to submit these requests while checking room membership, player registration,
state and ViewID. This is static API evidence, not a confirmed remote result.

`CharacterItems.SpawnItemInHand` sends its request to the Master Client. Its
handler creates the item and invokes PEAK pickup handling, so Clients can
submit item generation through that native path. Remote cleanse still needs
the owning character and has no reviewed network entrypoint. Dropped-item
provenance is recorded on the Master Client; recovery remains Host-only.

## 0.4.3 revival choices and independent cleanse

The installed PEAK `RPCA_ReviveAtPosition(position, applyStatus, segment)`
calls `ReviveCharacter(applyStatus)` on the receiving character. With `true`,
`ApplyPostReviveStatus` adds the native revival curse and Hunger 0.3. With
`false`, it skips those additions. The old 1.8.2 behavior used `false`, but
did not itself guarantee a full stamina refill.

`ReviveCharacter` clears afflictions and most statuses. It calls
`ClearAllStatus(true, true)`, so existing Curse/Petrify can remain. Both
revival choices are available to the Host for registered dead targets and
only change `applyStatus`. Neither explicitly refills stamina or promises to
remove all earlier negative effects. Live owner and remote results remain
unverified.

`SetStatus` and `AddStamina` return early for a non-owned PhotonView. A Host
cannot reliably clear a remote owner's statuses by writing its local replica.
The independent PlayerCleanse action uses `ClearAllAfflictions`,
`RemoveAllThorns` and local `ClearAll` on the owning living character. It
does not revive or change stamina. It clears injury, hunger, curse, hot/cold,
poison, spores and other curable statuses as well as physical cactus thorns;
Crab, Weight and Arrow are intentionally excluded by PEAK's curability rule.
The Client can use this local action, while other players' Cleanse buttons
stay disabled until a verified owner-side network path exists.

This clean-room review read metadata and IL from the installed PEAK
`Assembly-CSharp.dll` and `PhotonUnityNetworking.dll`. It did not load those
assemblies into PowerShell or copy any 1.8.2 method implementation.

## Current API evidence

- `Character.AllCharacters` contains the active non-bot character list.
  `IsPlayerControlled` excludes bots, zombies and Scoutmaster;
  `IsRegisteredToPlayer` verifies a current Photon actor mapping.
- `CharacterData.dead` and `fullyPassedOut` are separate state values.
  The UI offers Revive only for dead characters and Wake only for passed-out,
  living characters.
- PEAK's own `Character.Revive()` submits `RPCA_ReviveAtPosition` on the local
  character's PhotonView using `RpcTarget.All`, with a position, `true` and
  segment `-1`. The RPC invokes `ReviveCharacter` and `WarpPlayer`.
  The toolkit uses the target's public `LastLivingPosition` for revival.
- `Character.HandlePassedOut()` submits `RPCA_UnPassOut` on its PhotonView.
  The receiver runs its wake transition and clears `fullyPassedOut`.
- `Character.WarpToSpawn()` submits `WarpPlayerRPC` through Photon.
  `WarpPlayerRPC(Vector3, bool)` calls the game's `WarpPlayer`. The toolkit
  uses the destination character's `Center` and passes `false` for the visual
  effect flag.
- The installed Photon library exposes `PhotonNetwork.InRoom`,
  `IsMasterClient`, `PhotonView.ViewID`, `RpcTarget.All` and
  `PhotonView.RPC(string, RpcTarget, object[])`. No direct local replica
  status or transform writes are used.

## Guard and result boundary

Revive, Wake and Teleport require an active room, a registered local character,
current target-list membership and a positive target ViewID. Each action checks
its own API signature, so losing one API disables only that action. Revival,
wake and both teleport directions recheck state at click time. Disconnected
characters and bots are omitted from the directory.

Reflection returning from `PhotonView.RPC` proves only that the game RPC call
was invoked. It does not prove delivery or the owning client's outcome. The
UI therefore says to confirm the result in game. The actions need manual
Solo, Host and Client checks before claiming remote behavior is verified.
