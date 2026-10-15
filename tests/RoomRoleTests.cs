using System;
using PeakAdminToolkit.Core;

internal sealed class FakeRoomNetwork
{
    public static bool InRoom { get; set; }
    public static bool IsMasterClient { get; set; }
}
internal sealed class MissingRoomNetwork { public static bool IsMasterClient { get { return true; } } }

internal static class RoomRoleTests
{
    private static int checks;
    private static void Main()
    {
        try { Run(); Console.WriteLine("PASS: " + checks + " room-role checks."); }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex); Environment.ExitCode = 1; }
    }
    private static void Run()
    {
        var reader = new RoomRoleReader(name => name == "Photon.Pun.PhotonNetwork" ? typeof(FakeRoomNetwork) : null);
        FakeRoomNetwork.InRoom = false; FakeRoomNetwork.IsMasterClient = true;
        Check(reader.Read() == RoomRole.OutsideRoom, "master flag outside a room is not Host");
        FakeRoomNetwork.InRoom = true; FakeRoomNetwork.IsMasterClient = false;
        Check(reader.Read() == RoomRole.Client, "joined non-master is Client");
        FakeRoomNetwork.IsMasterClient = true;
        Check(reader.Read() == RoomRole.Host, "joined master is Host");
        Check(new RoomRoleReader(name => null).Read() == RoomRole.Unavailable, "missing Photon type is unknown");
        Check(new RoomRoleReader(name => typeof(MissingRoomNetwork)).Read() == RoomRole.Unavailable, "missing InRoom is unknown");
    }
    private static void Check(bool value, string name) { checks++; if (!value) throw new Exception(name); }
}
