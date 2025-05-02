namespace XenotypePatchUtils;

[StaticConstructorOnStartup]
internal class StaticBoi
{
    static StaticBoi()
    {
        int numPatched = PatchOperation_AutobalanceAll.NumXenotypesPatched;
        long timeTaken = PatchOperation_AutobalanceAll.TimeTaken;

        Log.Message($"[Xenotype Patch Utils] Patched {numPatched} xenotype{(numPatched != 1 ? "s" : "")} in {timeTaken}ms");
    }
}
