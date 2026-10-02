using System;
using PeakAdminToolkit.Core;

internal sealed class FakeRoomNetwork
{
    public static bool InRoom { get; set; }
    public static bool IsMasterClient { get; set; }
    public static bool OfflineMode { get; set; }
    public static FakeRoomPlayer LocalPlayer { get; set; }
    public static bool SubmitResult { get; set; }
    public static int TransferCalls;
    public static bool SetMasterClient(FakeRoomPlayer player) { TransferCalls++; return SubmitResult && ReferenceEquals(player, LocalPlayer); }
}
internal sealed class FakeRoomPlayer { }
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
        FakeRoomNetwork.LocalPlayer = new FakeRoomPlayer(); FakeRoomNetwork.SubmitResult = true;
        FakeRoomNetwork.InRoom = false; FakeRoomNetwork.IsMasterClient = true;
        Check(reader.Read() == RoomRole.OutsideRoom, "master flag outside a room is not Host");
        FakeRoomNetwork.InRoom = true; FakeRoomNetwork.IsMasterClient = false;
        Check(reader.Read() == RoomRole.Client, "joined non-master is Client");
        FakeRoomNetwork.IsMasterClient = true;
        Check(reader.Read() == RoomRole.Host, "joined master is Host");
        var transfer = new RoomHostTransfer(name => name == "Photon.Pun.PhotonNetwork" ? typeof(FakeRoomNetwork) : null);
        Check(!transfer.CanRequest(), "current Host does not submit self-transfer");
        FakeRoomNetwork.IsMasterClient = false;
        Check(transfer.CanRequest() && transfer.Request() && FakeRoomNetwork.TransferCalls == 1, "Client can submit native room transfer attempt");
        Check(reader.Read() == RoomRole.Client, "submitted request is not treated as confirmed Host");
        FakeRoomNetwork.SubmitResult = false;
        Check(!transfer.Request(), "false native return is reported as not submitted");
        FakeRoomNetwork.SubmitResult = true; FakeRoomNetwork.OfflineMode = true;
        Check(!transfer.CanRequest() && !transfer.Request(), "offline mode cannot transfer room role");
        FakeRoomNetwork.OfflineMode = false; FakeRoomNetwork.InRoom = false;
        Check(!transfer.CanRequest(), "outside room cannot request role transfer");
        FakeRoomNetwork.InRoom = true; FakeRoomNetwork.LocalPlayer = null;
        Check(!transfer.CanRequest(), "missing local Photon player cannot request role transfer");
        Check(!new RoomHostTransfer(name => null).CanRequest(), "missing Photon API isolates transfer action");
        Check(new RoomRoleReader(name => null).Read() == RoomRole.Unavailable, "missing Photon type is unknown");
        Check(new RoomRoleReader(name => typeof(MissingRoomNetwork)).Read() == RoomRole.Unavailable, "missing InRoom is unknown");
    }
    private static void Check(bool value, string name) { checks++; if (!value) throw new Exception(name); }
}
