namespace XenotypePatchUtils;

public class Settings : ModSettings
{
    public static bool devmode = false;

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref devmode, "devmode");
    }
}
