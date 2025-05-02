namespace XenotypePatchUtils;

// -efficiency since the metabolic efficiency impact of this gene is reversed when it's removed
public class ActionWorker_Remove(string defName, int efficiency) : ActionWorker(-efficiency)
{
    private readonly string defName = defName;

    public static ActionWorker Create(XmlNode action, XenotypeWorker xenotypeWorker)
    {
        XmlElement remove = action["remove"];

        if (remove == null)
        {
            // nothing to remove, always succeeds
            return Empty;
        }

        string defName = remove.InnerText;

        if (!xenotypeWorker.geneList.Has(defName, out int efficiency))
        {
            // gene list doesn't have gene, so we can't possibly remove it

            if (Settings.devmode)
            {
                XenotypePatchUtils.Message(xenotypeWorker.DefName, $"Removal of {defName} would not succeed - not in the genes list");
            }

            return null;
        }

        return new ActionWorker_Remove(defName, efficiency);
    }

    public override void Apply(XenotypeWorker xenotypeWorker)
    {
        if (Settings.devmode)
        {
            XenotypePatchUtils.Message(xenotypeWorker.DefName, $"Removing {defName} ({efficiencyChange.ToStringWithSign()})");
        }

        xenotypeWorker.geneList.Remove(defName);
    }
}
