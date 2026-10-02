# 0.4.2 Photon Host transfer attempt

The installed Photon PUN assembly exposes
`PhotonNetwork.SetMasterClient(Photon.Realtime.Player)`. It requires an online
room and forwards a room property update with an expected current Master
Client ID. The method does not require the local player to already be Host.
Its boolean return tells whether the transfer request was submitted, not
whether the server accepted it or the target became Host.

The Overview button is therefore enabled only for a Client in an online
room with a local Photon player. It requests transfer to that player and
shows a submission/failure message. The existing role reader still uses live
`InRoom` and `IsMasterClient`; dropped-item recovery continues to check
the live Host flag at click time. PEAK-specific scene or game state owned
by the previous Host is not handed over by this button. A two-player game
check is required before describing this as reliable Host migration.
