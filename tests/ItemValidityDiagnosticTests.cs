using System;
using System.Collections.Generic;
using PeakAdminToolkit.Core;
using PeakAdminToolkit.Items;

internal static class ItemValidityDiagnosticTests
{
    private static int checks;
    private static void Check(bool value, string name)
    {
        checks++;
        if (!value) throw new Exception(name);
    }

    private static PeakApi WithLog(Action<string> log)
    {
        return new PeakApi(new Compatibility(), null, log);
    }

    public static int Run()
    {
        ItemDatabase savedDatabase = ItemDatabase.Instance;
        bool savedHost = Photon.Pun.PhotonNetwork.IsMasterClient;
        int requests = Character.localCharacter.refs.items.Requests;
        try
        {
            ItemDatabase.Instance = new ItemDatabase();
            var rejected = new Item { name="Cursed Skull", Valid=false };
            var accepted = new Item { name="ScoutEffigy" };
            var throwing = new Item { name="Bugle_Magic", ThrowOnValidity=true };
            var wrongType = new WrongValidityItem { name="HealingDart Variant" };
            ItemDatabase.Instance.itemLookup.Add(rejected.name, rejected);
            ItemDatabase.Instance.itemLookup.Add(accepted.name, accepted);
            ItemDatabase.Instance.itemLookup.Add(throwing.name, throwing);
            ItemDatabase.Instance.itemLookup.Add(wrongType.name, wrongType);
            ItemDatabase.Instance.itemLookup.Add("Torch", new Item { name="Torch", Valid=false });
            var lines = new List<string>();
            PeakApi api = WithLog(lines.Add);
            var catalog = api.ReadItemCatalog(true);
            Check(lines.Exists(line => line.Contains("false: Cursed Skull")), "special catalog reports a measured false result");
            Check(lines.Exists(line => line.Contains("unknown: Bugle_Magic") && line.Contains("InvalidOperationException")), "invocation errors are unknown with a diagnostic reason");
            Check(lines.Exists(line => line.Contains("unknown: HealingDart Variant")), "incompatible return type is unknown");
            Check(!lines.Exists(line => line.Contains("false: Bugle_Magic") || line.Contains("false: HealingDart Variant")), "API failures are never reported as measured false");
            Check(lines.Exists(line => line.Contains("registered=4 true=1 false=1 unknown=2")), "summary counts only registered special candidates");
            Check(!lines.Exists(line => line.Contains("Torch")), "ordinary items are outside the special diagnostic");
            Check(catalog.Count == 2 && catalog.Exists(e => e.SpawnName == "ScoutEffigy") && catalog.Exists(e => e.SpawnName == "Cursed Skull"), "opt-in includes measured-false multiplayer item but excludes API errors");
            Check(rejected.ValidityCalls == 1 && accepted.ValidityCalls == 1 && throwing.ValidityCalls == 1, "diagnostic reuses each catalog validity evaluation");
            Check(Character.localCharacter.refs.items.Requests == requests, "diagnostic never submits a spawn request");

            ItemDatabase.Instance.itemLookup.Add("Parachute", null);
            lines.Clear();
            api.ReadItemCatalog(true);
            Check(lines.Exists(line => line.Contains("unknown: Parachute")), "missing registered prefab instance is unknown");
            ItemDatabase.Instance.itemLookup.Remove("Parachute");
            ItemDatabase.Instance.itemLookup.Add("SkullAlias", rejected);
            lines.Clear();
            api.ReadItemCatalog(true);
            Check(lines.Exists(line => line.Contains("registered=4 true=1 false=1 unknown=2")), "database aliases do not duplicate diagnostic counts");
            ItemDatabase.Instance.itemLookup.Remove("SkullAlias");

            lines.Clear();
            Check(api.ReadItemCatalog().Count == 1 && lines.Count == 0, "default browsing remains quiet and shows only the valid multiplayer candidate");
            rejected.Valid = true;
            lines.Clear();
            Check(api.ReadItemCatalog(true).Count == 2, "refresh observes a changed runtime result");
            Check(lines.Exists(line => line.Contains("false=0")) && !lines.Exists(line => line.Contains("false: Cursed Skull")), "refresh does not retain stale false names");

            var brokenLogger = WithLog(message => { throw new InvalidOperationException("log sink unavailable"); });
            Check(brokenLogger.ReadItemCatalog(true).Count == 2, "logging failure cannot hide otherwise valid entries");
            Photon.Pun.PhotonNetwork.IsMasterClient = false;
            lines.Clear();
            api.ReadItemCatalog(true);
            Check(lines.Count > 0 && api.CanSpawnItems && Character.localCharacter.refs.items.Requests == requests, "client diagnostics remain read-only while native spawn is available");

            ItemDatabase.Instance = null;
            lines.Clear();
            Check(api.ReadItemCatalog(true).Count == 0 && lines.Exists(line => line.Contains("unavailable")), "unavailable database is reported explicitly");
            Check(!lines.Exists(line => line.Contains("status=complete")), "unavailable database is not reported as a completed empty scan");
            ItemDatabase.Instance = new ItemDatabase();
            lines.Clear();
            api.ReadItemCatalog(true);
            Check(lines.Exists(line => line.Contains("registered=0 true=0 false=0 unknown=0")), "empty database explicitly reports zero observed candidates");

            bool valid;
            string reason;
            bool evaluated = ItemApiAccess.TryIsValidToSpawn(new object(), out valid, out reason);
            Check(!evaluated && !valid && reason.Contains("missing"), "missing validity API is unknown with a reason");
            Check(!ItemApiAccess.IsValidToSpawn(new object()), "missing API still blocks spawn eligibility");
            return checks;
        }
        finally
        {
            ItemDatabase.Instance = savedDatabase;
            Photon.Pun.PhotonNetwork.IsMasterClient = savedHost;
        }
    }
}

public sealed class WrongValidityItem : Item
{
    public new string IsValidToSpawn() { return "not a Boolean"; }
}
