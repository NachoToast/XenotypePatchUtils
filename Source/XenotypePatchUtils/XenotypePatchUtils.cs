global using RimWorld;
global using System.Diagnostics;
global using System.Xml;
global using UnityEngine;
global using Verse;

namespace XenotypePatchUtils;

public class XenotypePatchUtils : Mod
{
    public XenotypePatchUtils(ModContentPack content) : base(content)
    {
        GetSettings<Settings>();
    }

    public static void Error(string xenotypeDefName, string message)
    {
        Log.Error($"[Xenotype Patch Utils / {xenotypeDefName}] {message}");
    }

    public static void Message(string xenotypeDefName, string message)
    {
        Log.Message($"[Xenotype Patch Utils / {xenotypeDefName}] {message}");
    }

    public override string SettingsCategory()
    {
        return "XenotypePatchUtils.SettingsCategory".Translate();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);

        Listing_Standard listing = new();

        listing.Begin(inRect);

        listing.CheckboxLabeled("DevelopmentMode".Translate(), ref Settings.devmode, "XenotypePatchUtils.Tooltip".Translate());

        listing.End();
    }
}
