namespace XenotypePatchUtils;

public class ActionWorker_Add(string defName, int efficiency) : ActionWorker(efficiency)
{
    private readonly string defName = defName;

    private readonly int efficiency = efficiency;

    public static ActionWorker Create(XmlNode action, XenotypeWorker xenotypeWorker)
    {
        XmlElement add = action["add"];

        if (add == null)
        {
            // nothing to add, always succeeds
            return Empty;
        }

        string defName = add.InnerText;

        if (xenotypeWorker.geneList.Has(defName, out _))
        {
            // gene list already has gene, we shouldn't add it again

            if (Settings.devmode)
            {
                XenotypePatchUtils.Message(xenotypeWorker.DefName, $"Addition of {defName} would not succeed - already in the genes list");
            }

            return null;
        }

        if (!GeneDefResolver.TryGet(add, defName, out int efficiency))
        {
            // GeneDef does not exist, so we shouldn't add it

            if (Settings.devmode)
            {
                XenotypePatchUtils.Message(xenotypeWorker.DefName, $"Addition of {defName} would not succeed - GeneDef does not exist");
            }

            return null;
        }

        return new ActionWorker_Add(defName, efficiency);
    }

    public override void Apply(XenotypeWorker xenotypeWorker)
    {
        if (Settings.devmode)
        {
            XenotypePatchUtils.Message(xenotypeWorker.DefName, $"Adding {defName} ({efficiencyChange.ToStringWithSign()})");
        }

        xenotypeWorker.geneList.Add(defName, efficiency);
    }
}
