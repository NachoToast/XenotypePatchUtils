global using RimWorld;
global using System.Diagnostics;
global using System.Xml;
global using UnityEngine;
global using Verse;

namespace XenotypePatchUtils;

public class XenotypePatchUtils : Mod
{
    private static readonly Dictionary<string, Color> xenotypeColorMap = [];

    private static readonly IEnumerator<Color> colorGenerator = GetNextColor().GetEnumerator();

    private static IEnumerable<Color> GetNextColor()
    {
        while (true)
        {
            yield return Color.cyan.SaturationChanged(0.5f);
            yield return Color.magenta.SaturationChanged(0.5f);
        }
    }

    public XenotypePatchUtils(ModContentPack content) : base(content)
    {
        GetSettings<Settings>();
    }

    public static void Error(string xenotypeDefName, string message)
    {
        Log.Error($"[XPU:{xenotypeDefName.Colorize(GetColorFor(xenotypeDefName))}] {message}");
    }

    public static void Message(string xenotypeDefName, string message)
    {
        Log.Message($"[XPU:{xenotypeDefName.Colorize(GetColorFor(xenotypeDefName))}] {message}");
    }

    public override string SettingsCategory()
    {
        return "XenotypePatchUtils.SettingsCategory".Translate();
    }

    private static Color GetColorFor(string defName)
    {
        if (xenotypeColorMap.TryGetValue(defName, out Color value))
        {
            return value;
        }

        colorGenerator.MoveNext();

        value = colorGenerator.Current;

        xenotypeColorMap.Add(defName, value);

        return value;
    }

    public static string EfficiencyToString(int efficiency)
    {
        return efficiency.ToStringWithSign().Colorize(new(0.81f, 0.42f, 0.64f));
    }

    public static string EfficiencyToString(IntRange efficiencyRange)
    {
        return efficiencyRange.ToString().Colorize(new(0.81f, 0.42f, 0.64f));
    }

    public static string EfficiencyToColoredString(int efficiency)
    {
        if (efficiency < 0)
        {
            return efficiency.ToStringWithSign().Colorize(new Color(0.94f, 0.5f, 0.5f));
        }

        return efficiency.ToStringWithSign().Colorize(new Color(0.5f, 0.94f, 0.5f));
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
