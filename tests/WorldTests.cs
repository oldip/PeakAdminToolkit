using System;
using PeakAdminToolkit.World;

internal enum FakeSegment { Beach, Tropics, Alpine, Caldera, TheKiln, Peak, Void }
internal static class FakeNetwork { private static bool inRoom = true, master = true; public static bool InRoom { get { return inRoom; } set { inRoom = value; } } public static bool IsMasterClient { get { return master; } set { master = value; } } }
internal static class FakeGame { private static bool gameplay = true; public static bool IsInGameplayScene { get { return gameplay; } set { gameplay = value; } } }
internal sealed class FakeDay
{
    public static FakeDay instance = new FakeDay();
    public float timeOfDay = 9;
    public int dayCount = 1;
    private bool passedMidnight;
    public void MarkPendingMidnight() { passedMidnight = true; }
    public void ApplyDayUpdate() { if (passedMidnight && timeOfDay >= 5.5f) { dayCount++; passedMidnight = false; } }
    public static void SetTimeOfDay(float hour) { instance.timeOfDay = hour; }
}
internal sealed class FakeDayBadPending
{
    public static FakeDayBadPending instance = new FakeDayBadPending();
    private int passedMidnight = 1; public int Pending { get { return passedMidnight; } }
    public static void SetTimeOfDay(float hour) { }
} 
internal static class FakeMap
{
    private static bool initialized = true; public static bool ExistsAndInitialized { get { return initialized; } set { initialized = value; } }
    private static FakeSegment segment = FakeSegment.Beach; public static FakeSegment CurrentSegmentNumber { get { return segment; } set { segment = value; } }
    public static int Calls;
    public static void JumpToNextSegment() { Calls++; CurrentSegmentNumber++; }
}
internal static class FakeRun { public static float TimeSinceRunStarted = 5400; }

internal static class WorldTests
{
    private static int checks;
    private static Type Resolve(string name)
    {
        if (name == "Photon.Pun.PhotonNetwork") return typeof(FakeNetwork);
        if (name == "GameHandler") return typeof(FakeGame);
        if (name == "DayNightManager") return typeof(FakeDay);
        if (name == "MapHandler") return typeof(FakeMap);
        return null;
    }
    private static void Check(bool ok, string name) { checks++; if (!ok) throw new Exception(name); }
    private static void Main()
    {
        try { Run(); Console.WriteLine("PASS: " + checks + " world checks."); }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex); Environment.ExitCode = 1; }
    }
    private static void Run()
    {
        var time = new WorldTime(Resolve);
        Check(time.CanSet(), "Host in gameplay can set time");
        Check(time.Set(TimePreset.Morning) && FakeDay.instance.timeOfDay == 7, "morning is 07:00");
        Check(time.Set(TimePreset.Noon) && FakeDay.instance.timeOfDay == 12, "noon is 12:00");
        Check(time.Set(TimePreset.Evening) && FakeDay.instance.timeOfDay == 18, "evening is 18:00");
        Check(time.Set(TimePreset.Midnight) && FakeDay.instance.timeOfDay == 0, "midnight is 00:00");
        Check(FakeDay.instance.dayCount == 1 && FakeRun.TimeSinceRunStarted == 5400, "presets preserve day and run duration");
        int day; float hour;
        Check(time.TryRead(out day, out hour) && day == 1 && hour == 0, "current day and hour are read");
        FakeDay.instance.MarkPendingMidnight();
        Check(time.Set(TimePreset.Morning), "morning request accepted during pending native day change");
        FakeDay.instance.ApplyDayUpdate();
        Check(FakeDay.instance.dayCount == 1, "morning preset must not trigger pending native day increment");
        FakeNetwork.IsMasterClient = false;
        Check(!time.CanSet() && !time.Set(TimePreset.Noon), "Client cannot change time");
        Check(time.UnavailableReason() == "WorldHostRequired", "Client sees Host requirement");
        FakeNetwork.IsMasterClient = true; FakeGame.IsInGameplayScene = false;
        Check(!time.CanSet() && time.UnavailableReason() == "WorldGameplayRequired", "time unavailable outside gameplay");
        FakeGame.IsInGameplayScene = true; FakeDay.instance = null;
        Check(!time.CanSet() && time.UnavailableReason() == "WorldTimeApiUnavailable", "missing day manager disables only time");
        FakeDay.instance = new FakeDay();
        Check(!new WorldTime(name => name == "DayNightManager" ? null : Resolve(name)).CanSet(), "changed time API fails closed");
        Check(!new WorldTime(name => name == "DayNightManager" ? typeof(FakeDayBadPending) : Resolve(name)).CanSet(), "changed pending-midnight field type fails closed");
        Check(new WorldTime(name => name == "Photon.Pun.PhotonNetwork" ? null : Resolve(name)).UnavailableReason() == "WorldTimeApiUnavailable", "missing network API is not reported as Client");
        FakeNetwork.InRoom = false;
        Check(!time.CanSet(), "outside room cannot change time");
        FakeNetwork.InRoom = true;

        var advance = new WorldAdvance(Resolve);
        string current, next;
        Check(advance.CanAdvance() && advance.TryRead(out current, out next) && current == "Beach" && next == "Tropics", "Beach destination is Tropics");
        Check(advance.Advance() && WorldDestinationTeleport.Calls == 1 && FakeMap.Calls == 0 && FakeMap.CurrentSegmentNumber == FakeSegment.Beach, "Beach dispatches warp without native jump or progress change");
        FakeMap.CurrentSegmentNumber = FakeSegment.TheKiln;
        Check(advance.TryRead(out current, out next) && next == "Peak" && advance.CanAdvance(), "TheKiln to Peak allowed");
        int nativeCallsBeforePeak = FakeMap.Calls;
        Check(advance.Advance() && WorldDestinationTeleport.Calls == 2 && FakeMap.Calls == nativeCallsBeforePeak,
            "TheKiln uses summit warp without native debug jump");
        WorldDestinationTeleport.Result = false;
        Check(!advance.Advance() && FakeMap.Calls == nativeCallsBeforePeak, "summit landing failure leaves native jump unused");
        WorldDestinationTeleport.Result = true;
        FakeMap.CurrentSegmentNumber = FakeSegment.Peak;
        Check(!advance.CanAdvance() && !advance.Advance() && advance.UnavailableReason() == "WorldNoNextSegment", "Peak to Void blocked");
        int callsAtEnd = WorldDestinationTeleport.Calls;
        FakeMap.CurrentSegmentNumber = FakeSegment.Void;
        Check(advance.CanAdvance() && advance.TryRead(out current, out next) && next == "Void", "Void endpoint stays in the current area");
        Check(advance.Advance(), "Void endpoint warp is available");
        Check(WorldDestinationTeleport.Calls == callsAtEnd + 1, "Void submits one endpoint warp");
        WorldDestinationTeleport.Result = false;
        Check(!advance.Advance() && FakeMap.CurrentSegmentNumber == FakeSegment.Void && FakeMap.Calls == nativeCallsBeforePeak, "failed endpoint warp never advances progress");
        WorldDestinationTeleport.Result = true;
        FakeNetwork.IsMasterClient = false;
        Check(!advance.CanAdvance() && !advance.Advance(), "Client cannot move team to endpoint");
        FakeNetwork.IsMasterClient = true;
        FakeMap.CurrentSegmentNumber = FakeSegment.Beach; FakeNetwork.IsMasterClient = false;
        Check(!advance.CanAdvance() && !advance.Advance() && advance.UnavailableReason() == "WorldHostRequired", "Client cannot advance team");
        FakeNetwork.IsMasterClient = true; FakeGame.IsInGameplayScene = false;
        Check(!advance.CanAdvance(), "jump unavailable outside gameplay");
        FakeGame.IsInGameplayScene = true; FakeMap.ExistsAndInitialized = false;
        Check(!advance.CanAdvance(), "uninitialized map cannot advance");
        FakeMap.ExistsAndInitialized = true;
        Check(!new WorldAdvance(name => name == "MapHandler" ? null : Resolve(name)).CanAdvance(), "changed map API fails closed");
        Check(new WorldAdvance(name => name == "Photon.Pun.PhotonNetwork" ? null : Resolve(name)).UnavailableReason() == "WorldMapApiUnavailable", "missing network API is not reported as Client");
        Check(time.CanSet(), "missing map API does not disable time");

        int sent = 0;
        string[] players = { "one", "two" };
        Check(!WorldWarpBatch.TryWarp(players, player => true, index => index == 1 ? null : (object)"safe",
            (player, position) => { sent++; return true; }) && sent == 0,
            "missing second landing sends no warp");
        Check(!WorldWarpBatch.TryWarp(players, player => player != "two", index => (object)"safe",
            (player, position) => { sent++; return true; }) && sent == 0,
            "missing player RPC sends no warp");
        Check(WorldWarpBatch.TryWarp(players, player => true, index => (object)("point" + index),
            (player, position) => { sent++; return true; }) && sent == 2,
            "all prepared players receive one warp");
    }
}










namespace PeakAdminToolkit.World
{
    internal static class WorldDestinationTeleport
    {
        internal static int Calls;
        internal static bool Result = true;
        internal static bool TryTeleport(string current, string next) { Calls++; return Result; }
    }
}
