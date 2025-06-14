namespace XenotypePatchUtils;

[StaticConstructorOnStartup]
internal class StaticBoi
{
    static StaticBoi()
    {
        int numPatched = PatchOperation_AutobalanceAll.NumXenotypesPatched;
        long timeTaken = PatchOperation_AutobalanceAll.TimeTaken;

        string modName = "Xenotype Patch Utils".Colorize(ColoredText.ImpactColor);

        Log.Message($"[{modName}] Patched {numPatched} xenotype{(numPatched != 1 ? "s" : "")} in {timeTaken}ms");

        GeneDefResolver.Clear(out int cachedGeneCount, out int missingGeneCount);

        if (Settings.devmode)
        {
            Log.Message($"[{modName}] Cleared cache ({cachedGeneCount} resolved, {missingGeneCount} missing)");
        }
    }
}
