using System;
using PeakAdminToolkit.Players;

internal static class PlayerTests
{
    private static int checks;
    private static void Main()
    {
        try { Run(); Console.WriteLine("PASS: " + checks + " player-management checks."); }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex); Environment.ExitCode = 1; }
    }
    private static void Run()
    {
        var self = new FakePlayerCharacter { characterName = "Host", Center = new PlayerVector(1,2,3) };
        var friend = new FakePlayerCharacter { characterName = "Friend", Center = new PlayerVector(10,11,12), LastLivingPosition = new PlayerVector(8,9,10) };
        FakePlayerCharacter.AllCharacters.Clear(); FakePlayerCharacter.AllCharacters.Add(self); FakePlayerCharacter.AllCharacters.Add(friend);
        FakePlayerCharacter.localCharacter = self;
        FakeNetwork.InRoom = true; FakeNetwork.IsMasterClient = true;
        var directory = new PlayerDirectory(Resolve);
        var landing = new FakeLanding();
        object floor = new object(), wall = new object();
        Check(LandingClearance.CanStand(new object[] { floor }, floor), "supporting floor does not block standing space");
        Check(!LandingClearance.CanStand(new object[] { floor, wall }, floor), "other obstacle blocks standing space");
        Check(LandingClearance.CanStand(new object[0], floor), "empty standing space is usable");
        Check(LandingClearance.CanStand(new object[] { new FakeCollider(7) }, new FakeCollider(7)), "same native floor with different wrapper is ignored");
        var revive = new PlayerRevive(directory, landing); var cleanRevive = new PlayerNoPenaltyRevive(directory, landing); var cleanse = new PlayerCleanse(directory, Resolve); var teleport = new PlayerTeleport(directory, landing);
        var players = directory.Read();
        Check(players.Count == 2 && players[0].Name == "Host" && players[1].Name == "Friend", "roster lists current human characters");
        var bot = new FakePlayerCharacter { characterName = "Bot", isBot = true };
        var disconnected = new FakePlayerCharacter { characterName = "Disconnected", IsRegisteredToPlayer = false };
        FakePlayerCharacter.AllCharacters.Add(bot); FakePlayerCharacter.AllCharacters.Add(disconnected);
        Check(directory.Read().Count == 2, "bots and unregistered characters are hidden");
        FakePlayerCharacter.AllCharacters.Remove(bot); FakePlayerCharacter.AllCharacters.Remove(disconnected);
        Check(!revive.CanRequest(players[1]), "living player cannot be revived");
        friend.data.dead = true;
        Check(revive.CanRequest(players[1]), "dead player can be revived");
        Check(revive.Request(players[1]), "host can submit revive for remote player");
        Check(cleanRevive.CanRequest(players[1]) && cleanRevive.Request(players[1]), "host can request no-penalty revive for remote player");
        Check(friend.photonView.Calls.Count == 2 && friend.photonView.Calls[0] == "RPCA_ReviveAtPosition" &&
            friend.photonView.Calls[1] == "RPCA_ReviveAtPosition", "both revive choices use target view RPC");
        Check(((PlayerVector)friend.photonView.LastArguments[0]).x == 3 && !(bool)friend.photonView.LastArguments[1] && (int)friend.photonView.LastArguments[2] == -1, "remote no-penalty revive beside operator skips post-revive status");
        landing.Allow = false;
        Check(revive.Request(players[1]) && ((PlayerVector)friend.photonView.LastArguments[0]).x == 5, "when no safe floor is found remote revival uses operator facing direction");
        landing.Allow = true;
        self.data.dead = true;
        friend.data.dead = false;
        FakeSpectator.specCharacter = friend;
        Check(revive.Request(players[0]) && self.photonView.Calls[0] == "RPCA_ReviveAtPosition", "host can revive own dead character separately");
        Check(cleanRevive.CanRequest(players[0]) && cleanRevive.Request(players[0]), "host can request no-penalty revive for own dead character");
        Check(((PlayerVector)self.photonView.LastArguments[0]).x == 12, "own ghost revives beside currently spectated living friend");
        landing.Allow = false;
        Check(cleanRevive.Request(players[0]) && ((PlayerVector)self.photonView.LastArguments[0]).x == 14, "ghost revival fallback uses spectated friend's facing direction");
        landing.Allow = true;
        Check(!(bool)self.photonView.LastArguments[1] && (int)self.photonView.LastArguments[2] == -1, "clean revival skips native post-revive penalties");
        FakeSpectator.specCharacter = null;
        Check(revive.Request(players[0]) && ((PlayerVector)self.photonView.LastArguments[0]).x == self.LastLivingPosition.x, "own ghost without spectated teammate revives at original position");
        Check(self.stamina == 0.25f && self.HasDebuff && FakePlayerCharacter.ClearAllCalls == 0,
            "no-penalty revive does not secretly cleanse or refill stamina");
        self.data.dead = false;
        friend.data.dead = false; friend.data.fullyPassedOut = true;
        Check(cleanse.CanRequest(players[0]) && !cleanse.CanRequest(players[1]), "cleanse is independent and local-owner only");
        FakeNetwork.IsMasterClient = false;
        Check(cleanse.Request(players[0]) && !self.HasDebuff && FakePlayerCharacter.ClearAllCalls == 1 &&
            FakeAfflictions.ThornCalls == 1 && FakeAfflictions.AfflictionCalls == 1,
            "Client may clear own thorns, statuses and afflictions without reviving or filling stamina");
        Check(self.stamina == 0.25f && self.photonView.Calls.Count == 4, "cleanse leaves stamina and revive RPC history unchanged");
        FakeNetwork.IsMasterClient = true;
        Check(!revive.CanRequest(players[1]), "passed-out living player cannot be revived");
        friend.data.fullyPassedOut = false;
        Check(teleport.CanRequestTo(players[1]) && teleport.CanRequestBring(players[1]), "both teleport directions available for living players");
        Check(teleport.RequestTo(players[1]), "self to friend request submitted");
        Check(self.photonView.Calls[self.photonView.Calls.Count-1] == "WarpPlayerRPC" && ((PlayerVector)self.photonView.LastArguments[0]).x == 12, "self view moves beside friend");
        Check(teleport.RequestBring(players[1]), "friend to self request submitted");
        Check(friend.photonView.Calls[friend.photonView.Calls.Count-1] == "WarpPlayerRPC" && ((PlayerVector)friend.photonView.LastArguments[0]).x == 3, "friend view moves beside self");
        landing.Allow = false;
        int before = self.photonView.Calls.Count;
        Check(!teleport.RequestTo(players[1]) && self.photonView.Calls.Count == before, "no safe adjacent floor sends no teleport RPC");
        landing.Allow = true;
        FakeNetwork.IsMasterClient = false;
        friend.data.dead = true;
        Check(revive.Request(players[1]) && cleanRevive.Request(players[1]), "client can try both remote revival choices");
        friend.data.dead = false;
        Check(teleport.RequestTo(players[1]) && teleport.RequestBring(players[1]), "client can try both teleport directions");
        self.data.dead = true;
        Check(revive.Request(players[0]) && cleanRevive.Request(players[0]), "client can revive own character");
        self.data.dead = false;
        FakeNetwork.IsMasterClient = true;
        FakeNetwork.InRoom = false;
        Check(!teleport.RequestTo(players[1]), "outside room denied");
        FakeNetwork.InRoom = true;
        FakePlayerCharacter.AllCharacters.Remove(friend);
        Check(!teleport.RequestBring(players[1]), "stale target denied");
        FakePlayerCharacter.AllCharacters.Add(friend);
        FakePlayerCharacter.AllCharacters.Remove(self);
        Check(!revive.Request(players[1]) && !teleport.RequestBring(players[1]), "unregistered local character cannot manage players");
        FakePlayerCharacter.AllCharacters.Insert(0, self);
        friend.photonView.ViewID = 0;
        Check(!teleport.RequestBring(players[1]) && !revive.Request(players[1]), "unregistered target view denied");
        friend.photonView.ViewID = 1;
        self.data.dead = true;
        Check(!teleport.RequestTo(players[1]) && !teleport.RequestBring(players[1]), "dead local player cannot teleport");
        self.data.dead = false;
        Check(revive.Available && cleanRevive.Available && cleanse.Available && teleport.Available, "each capability bound separately");
        self.data.dead = true;
        Check(!cleanse.CanRequest(players[0]) && !cleanse.Request(players[0]), "cleanse never acts as revival");
        self.data.dead = false;
        var missing = new PlayerDirectory(name => name == "Character" ? typeof(FakePlayerCharacterWithoutRevive) : Resolve(name));
        Check(!new PlayerRevive(missing).Available && new PlayerTeleport(missing).Available,
            "missing revive API disables only revive");
        Check(!new PlayerNoPenaltyRevive(missing).Available, "no-penalty revive requires native revive API");
        Check(!new PlayerCleanse(directory, name => null).Available && revive.Available, "missing cleanse API disables only cleanse");
        string reason = FailureReason(new PlayerRevive(missing));
        Check(reason != null && reason.Contains("RPCA_ReviveAtPosition"), "revival incompatibility identifies its native method");
        Check(FailureReason(new PlayerNoPenaltyRevive(missing)) == reason, "both revival choices report the same API failure");
        reason = FailureReason(new PlayerTeleport(new PlayerDirectory(name => typeof(object))));
        Check(reason != null && reason.Contains("Center"), "teleport incompatibility identifies its missing position member");
        reason = FailureReason(new PlayerCleanse(directory, name => null));
        Check(reason != null && reason.Contains("CharacterAfflictions"), "cleanse incompatibility identifies its missing type");
        Check(FailureReason(revive) == null && FailureReason(teleport) == null && FailureReason(cleanse) == null, "available player modules have no failure reason");
    }
    private static string FailureReason(object feature)
    {
        var property = feature.GetType().GetProperty("FailureReason", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return property == null ? null : property.GetValue(feature, null) as string;
    }
    private static Type Resolve(string name)
    {
        if (name == "Character") return typeof(FakePlayerCharacter);
        if (name == "Photon.Pun.PhotonNetwork") return typeof(FakeNetwork);
        if (name == "Photon.Pun.RpcTarget") return typeof(FakeRpcTarget);
        if (name == "CharacterAfflictions") return typeof(FakeAfflictions);
        if (name == "MainCameraMovement") return typeof(FakeSpectator);
        return null;
    }
    private static void Check(bool value, string name) { checks++; if (!value) throw new Exception(name); }
}
