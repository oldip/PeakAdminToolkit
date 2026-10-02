using System;
using System.Collections.Generic;

internal struct PlayerVector { public float x, y, z; public PlayerVector(float x, float y, float z) { this.x=x; this.y=y; this.z=z; } }
internal sealed class FakeCollider
{
    private readonly int id;
    internal FakeCollider(int id) { this.id = id; }
    public override bool Equals(object other) { return other is FakeCollider && ((FakeCollider)other).id == id; }
    public override int GetHashCode() { return id; }
}
internal sealed class FakeSpectator { public static FakePlayerCharacter specCharacter { get; set; } }
internal sealed class FakeLanding : PeakAdminToolkit.Players.IPlayerLanding
{
    public bool Allow = true;
    public bool TryNear(object anchor, out object position)
    {
        PlayerVector center = ((FakePlayerCharacter)anchor).Center;
        position = new PlayerVector(center.x + 2, center.y, center.z);
        return Allow;
    }
    public bool TryInFront(object anchor, out object position)
    {
        PlayerVector center = ((FakePlayerCharacter)anchor).Center;
        position = new PlayerVector(center.x + 4, center.y, center.z);
        return true;
    }
}
internal enum FakeRpcTarget { All, AllViaServer }
internal sealed class FakeNetwork
{
    public static bool InRoom { get; set; }
    public static bool IsMasterClient { get; set; }
}
internal sealed class FakePlayerView
{
    public int ViewID = 1;
    public int OwnerActorNr { get; set; }
    public bool IsMine { get; set; }
    public readonly List<string> Calls = new List<string>();
    public object[] LastArguments;
    public object Component;
    public object GetComponent(Type type) { return Component != null && type.IsInstanceOfType(Component) ? Component : null; }
    public void RPC(string method, FakeRpcTarget target, params object[] arguments)
    {
        Calls.Add(method); LastArguments = arguments;
    }
}
internal sealed class FakePlayerData
{
    public bool dead { get; set; }
    public bool fullyPassedOut;
}
internal sealed class FakeAfflictions
{
    public static int ThornCalls;
    public static int AfflictionCalls;
    public static void ClearAll() { FakePlayerCharacter.ClearAllCalls++; FakePlayerCharacter.localCharacter.HasDebuff = false; }
    public void RemoveAllThorns() { ThornCalls++; }
    public void ClearAllAfflictions() { AfflictionCalls++; }
}
internal sealed class FakePlayerItems { public readonly List<FakePlayerView> droppedItems = new List<FakePlayerView>(); }
internal sealed class FakePlayerRefs { public FakeAfflictions afflictions = new FakeAfflictions(); public FakePlayerItems items = new FakePlayerItems(); }
internal class FakePlayerCharacter
{
    public static readonly List<FakePlayerCharacter> AllCharacters = new List<FakePlayerCharacter>();
    public static FakePlayerCharacter localCharacter;
    public string characterName { get; set; }
    public bool IsPlayerControlled { get; set; }
    public bool IsRegisteredToPlayer { get; set; }
    public bool isBot = false;
    public FakePlayerData data = new FakePlayerData();
    public FakePlayerRefs refs = new FakePlayerRefs();
    public FakePlayerView photonView = new FakePlayerView();
    public PlayerVector Center { get; set; }
    public PlayerVector LastLivingPosition { get; set; }
    public float stamina = 0.25f;
    public bool HasDebuff = true;
    public static int ClearAllCalls;
    public FakePlayerCharacter() { IsPlayerControlled = true; IsRegisteredToPlayer = true; photonView.IsMine = true; }
    private void RPCA_ReviveAtPosition(PlayerVector position, bool applyStatus, int segment) { }
    private void RPCA_UnPassOut() { }
    public void WarpPlayerRPC(PlayerVector position, bool poof) { }
}
internal sealed class FakePlayerCharacterWithoutRevive
{
    public static readonly List<FakePlayerCharacterWithoutRevive> AllCharacters = new List<FakePlayerCharacterWithoutRevive>();
    public static FakePlayerCharacterWithoutRevive localCharacter = new FakePlayerCharacterWithoutRevive();
    public string characterName { get; set; }
    public bool IsPlayerControlled { get; set; }
    public FakePlayerData data = new FakePlayerData();
    public FakePlayerView photonView = new FakePlayerView();
    public PlayerVector Center { get; set; }
    public PlayerVector LastLivingPosition { get; set; }
    private void RPCA_UnPassOut() { }
    public void WarpPlayerRPC(PlayerVector position, bool poof) { }
}
internal enum FakeItemState { Ground, Held, InBackpack }
internal sealed class FakeItemTransform { public object rotation = new object(); }
internal sealed class FakeRecoveryItem
{
    public FakeItemState itemState { get; set; }
    public FakePlayerView photonView;
    public FakeItemTransform transform = new FakeItemTransform();
    public string Name;
    public string GetName() { return Name; }
    public void SetKinematicRPC(bool value, PlayerVector position, object rotation) { }
    public FakeRecoveryItem(string name, int id, bool owned = true)
    {
        Name = name; itemState = FakeItemState.Ground; photonView = new FakePlayerView { ViewID = id, IsMine = owned, Component = this };
    }
}
