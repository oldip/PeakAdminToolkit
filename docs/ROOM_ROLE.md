# 0.4.1 room-role evidence

Overview reads the installed Photon PUN `PhotonNetwork.InRoom` and
`IsMasterClient` properties. Outside a room it displays a separate state,
even if the master flag happens to be true. Missing APIs display unavailable.
The player-management action gate remains in `PlayerDirectory.CanManage` and
requires both values to be true.

The installed Photon library also exposes `PhotonNetwork.SetMasterClient(Player)`.
It forwards a request to the room and returns whether that request was sent;
it does not prove the Master Client changed. Photon's official PUN 2
[host-migration guide](https://doc.photonengine.com/pun/current/gameplay/hostmigration)
says the new Master Client does not automatically receive all state from the
former one. PEAK's transfer behavior has not been tested with multiple players,
so 0.4.1 does not offer a force-transfer button.
