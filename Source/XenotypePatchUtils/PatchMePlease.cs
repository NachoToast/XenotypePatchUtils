namespace XenotypePatchUtils;

public class PatchMePlease : DefModExtension
{
    protected readonly IntRange desiredEfficiencyRange;

    protected readonly List<AlwaysRule> always;

    protected readonly List<ToFixMetabolismRule> toFixMetabolism;

    protected class AlwaysRule
    {
        protected readonly GeneDef remove;

        protected readonly GeneDef add;

        protected readonly List<GeneDef> addFirst;
    }

    protected class ToFixMetabolismRule : AlwaysRule
    {
        protected readonly List<GeneDef> addBest;
    }
}
