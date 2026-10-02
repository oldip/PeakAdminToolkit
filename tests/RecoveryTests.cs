using System;
using System.Collections.Generic;
using PeakAdminToolkit.Players;

internal static class RecoveryTests
{
    private static int checks;
    private static void Main()
    {
        try { Run(); Console.WriteLine("PASS: " + checks + " dropped-item recovery checks."); }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex); Environment.ExitCode = 1; }
    }
    private static void Run()
    {
        var host = new FakePlayerCharacter { characterName = "Host", Center = new PlayerVector(1, 2, 3) };
        var friend = new FakePlayerCharacter { characterName = "Friend" };
        FakePlayerCharacter.AllCharacters.Clear(); FakePlayerCharacter.AllCharacters.Add(host); FakePlayerCharacter.AllCharacters.Add(friend);
        FakePlayerCharacter.localCharacter = host; FakeNetwork.InRoom = true; FakeNetwork.IsMasterClient = true;
        host.data.fullyPassedOut = false;
        var directory = new PlayerDirectory(Resolve);
        var history = new DroppedItemHistory();
        var recovery = new DroppedItemRecovery(directory, Resolve, history);
        var target = directory.Read()[1];
        var bag = Add(friend, "Backpack", 101);
        var compass = Add(friend, "Compass", 102);
        friend.refs.items.droppedItems.Add(bag.photonView);
        var held = Add(friend, "Held", 103); held.itemState = FakeItemState.Held;
        Add(friend, "Client object", 104, false);
        var invalid = Add(friend, "No view", 0);
        var other = Add(host, "Host object", 105);
        Check(recovery.Available, "API available independently");
        var found = recovery.Read(target);
        Check(found.Count == 2 && found[0].ViewId == 101 && found[1].ViewId == 102, "only distinct, current, owned ground candidates");
        Check(found[0].Name == "Backpack", "localized live item name");
        var earlyHandDrop = Add(friend, "Hand drop before passout", 106);
        history.Record(friend, earlyHandDrop.photonView);
        friend.refs.items.droppedItems.Clear();
        found = recovery.Read(target);
        Check(found.Count == 1 && found[0].ViewId == 106, "hand drop remains selectable after native passout clears droppedItems");
        Check(recovery.MoveSelected(target, new[] { 106 }) == 1, "recorded original hand drop can be moved");
        friend.refs.items.droppedItems.Add(bag.photonView);
        friend.refs.items.droppedItems.Add(compass.photonView);
        Check(recovery.MoveSelected(target, new[] { 101, 105 }) == 1, "only selected target provenance moved");
        Check(bag.photonView.Calls.Count == 1 && bag.photonView.Calls[0] == "SetKinematicRPC", "original backpack moved by item RPC");
        Check(bag.photonView.LastArguments.Length == 3 && !(bool)bag.photonView.LastArguments[0], "ground state retained");
        Check(((PlayerVector)bag.photonView.LastArguments[1]).x > host.Center.x, "destination beside living host");
        Check(other.photonView.Calls.Count == 0, "another player's item untouched");
        compass.itemState = FakeItemState.Held;
        Check(recovery.MoveSelected(target, new[] { 102 }) == 0, "pickup after selection is rejected");
        Check(held.photonView.Calls.Count == 0 && invalid.photonView.Calls.Count == 0, "non-ground and invalid views untouched");
        FakeNetwork.IsMasterClient = false;
        Check(recovery.Read(target).Count == 0 && recovery.MoveSelected(target, new[] { 101 }) == 0, "Client cannot enumerate or move");
        FakeNetwork.IsMasterClient = true; host.data.dead = true;
        Check(recovery.MoveSelected(target, new[] { 101 }) == 0, "dead host cannot receive items");
        host.data.dead = false; FakePlayerCharacter.AllCharacters.Remove(friend);
        Check(recovery.MoveSelected(target, new[] { 101 }) == 0, "stale player rejected");
    }
    private static FakeRecoveryItem Add(FakePlayerCharacter owner, string name, int id, bool owned = true)
    {
        var item = new FakeRecoveryItem(name, id, owned);
        owner.refs.items.droppedItems.Add(item.photonView);
        return item;
    }
    private static Type Resolve(string name)
    {
        if (name == "Character") return typeof(FakePlayerCharacter);
        if (name == "Item") return typeof(FakeRecoveryItem);
        if (name == "Photon.Pun.PhotonNetwork") return typeof(FakeNetwork);
        if (name == "Photon.Pun.RpcTarget") return typeof(FakeRpcTarget);
        return null;
    }
    private static void Check(bool ok, string name) { checks++; if (!ok) throw new Exception(name); }
}
