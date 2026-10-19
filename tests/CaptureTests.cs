using System;
using PeakAdminToolkit.Players;

internal sealed class CharacterItems
{
    private readonly object character;
    internal CharacterItems(object character) { this.character = character; }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public void DropItemRpc(float throwCharge, byte slotID, object spawnPos, object velocity,
        object rotation, object itemInstanceData, bool cacheToDroppedItems)
    { Photon.Pun.PhotonNetwork.InstantiateItemRoom("drop", null, null, false); }
}

internal sealed class FakeGameObject
{
    private readonly Photon.Pun.PhotonView view = new Photon.Pun.PhotonView();
    public object GetComponent(Type type) { return type == typeof(Photon.Pun.PhotonView) ? view : null; }
    internal Photon.Pun.PhotonView View { get { return view; } }
}

namespace Photon.Pun
{
    internal sealed class PhotonView { }
    internal static class PhotonNetwork
    {
        internal static FakeGameObject Last;
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static FakeGameObject InstantiateItemRoom(string name, object position, object rotation, bool data)
        { return Last = new FakeGameObject(); }
    }
}

internal static class CaptureTests
{
    private static int checks;
    private static void Main()
    {
        try { Run(); Console.WriteLine("PASS: " + checks + " hand-drop Harmony checks."); }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex.Message); Environment.ExitCode = 1; }
    }
    private static void Run()
    {
        object player = new object();
        var history = new DroppedItemHistory();
        using (var capture = new DroppedItemCapture(history, message => { throw new Exception(message); }))
        {
            Check(capture.Available, "native hook installed");
            var items = new CharacterItems(player);
            items.DropItemRpc(0, 0, null, null, null, null, false);
            Check(Count(history, player) == 0, "voluntary drop before death is not a recovery candidate");
            items.DropItemRpc(0, 0, null, null, null, null, true);
            object automatic = Photon.Pun.PhotonNetwork.Last.View;
            int count = 0;
            foreach (object view in history.Read(player))
            {
                Check(ReferenceEquals(view, Photon.Pun.PhotonNetwork.Last.View), "spawned hand item belongs to drop owner");
                count++;
            }
            Check(count == 1, "one hand drop captured independently of game droppedItems");
            items.DropItemRpc(1, 0, null, null, null, null, false);
            Check(Count(history, player) == 1, "voluntary throw after automatic drop does not expand recovery list");
            Check(ReferenceEquals(First(history, player), automatic), "original automatic drop remains selectable");
            object friend = new object();
            var friendItems = new CharacterItems(friend);
            friendItems.DropItemRpc(0, 0, null, null, null, null, false);
            Check(Count(history, friend) == 0, "another player's voluntary drop is excluded");
            friendItems.DropItemRpc(0, 0, null, null, null, null, true);
            Check(Count(history, friend) == 1 && Count(history, player) == 1, "automatic drops remain assigned to their owner");
            Photon.Pun.PhotonNetwork.InstantiateItemRoom("unrelated spawn", null, null, false);
            Check(Count(history, player) == 1 && Count(history, friend) == 1, "unrelated item spawning is not captured");
            Check(Count(history, player) == 1, "automatic drop is retained without a short recovery timeout");
        }
        new CharacterItems(player).DropItemRpc(0, 0, null, null, null, null, true);
        Check(Count(history, player) == 1, "disposed capture no longer records drops");
        history.Clear();
        Check(!history.Read(player).GetEnumerator().MoveNext(), "scene transition clears captured provenance");
    }
    private static int Count(DroppedItemHistory history, object player)
    { int count = 0; foreach (object view in history.Read(player)) count++; return count; }
    private static object First(DroppedItemHistory history, object player)
    { foreach (object view in history.Read(player)) return view; return null; }
    private static void Check(bool ok, string name) { checks++; if (!ok) throw new Exception(name); }
}
