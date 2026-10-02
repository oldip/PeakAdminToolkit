using System;
using PeakAdminToolkit.Players;

internal sealed class CharacterItems
{
    private readonly object character;
    internal CharacterItems(object character) { this.character = character; }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public void DropItemRpc() { Photon.Pun.PhotonNetwork.InstantiateItemRoom("drop", null, null, false); }
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
    private static void Main()
    {
        object player = new object();
        var history = new DroppedItemHistory();
        using (var capture = new DroppedItemCapture(history, message => { throw new Exception(message); }))
        {
            Check(capture.Available, "native hook installed");
            new CharacterItems(player).DropItemRpc();
            int count = 0;
            foreach (object view in history.Read(player))
            {
                Check(ReferenceEquals(view, Photon.Pun.PhotonNetwork.Last.View), "spawned hand item belongs to drop owner");
                count++;
            }
            Check(count == 1, "one hand drop captured independently of game droppedItems");
        }
        history.Clear();
        Check(!history.Read(player).GetEnumerator().MoveNext(), "scene transition clears captured provenance");
        Console.WriteLine("PASS: 4 hand-drop Harmony checks.");
    }
    private static void Check(bool ok, string name) { if (!ok) throw new Exception(name); }
}
