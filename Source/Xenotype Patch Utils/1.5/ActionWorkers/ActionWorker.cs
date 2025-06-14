namespace XenotypePatchUtils;

public abstract class ActionWorker(int efficiencyChange)
{
    /// <summary>
    /// An action worker that does nothing.
    /// </summary>
    public static ActionWorker Empty = new ActionWorker_Empty();

    /// <summary>
    /// Total metabolic efficiency change should this action be applied.
    /// </summary>
    public readonly int efficiencyChange = efficiencyChange;

    public abstract void Apply(XenotypeWorker xenotypeWorker);
}
