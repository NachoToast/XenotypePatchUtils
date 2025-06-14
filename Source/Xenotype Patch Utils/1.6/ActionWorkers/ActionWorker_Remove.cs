using System.Text;

namespace XenotypePatchUtils;

// -efficiency since the metabolic efficiency impact of this gene is reversed when it's removed
public class ActionWorker_Remove(string defName, int efficiency) : ActionWorker(-efficiency)
{
    private readonly string defName = defName;

    public static bool TryCreate(XmlNode action, XenotypeWorker xenotypeWorker, out ActionWorker worker)
    {
        XmlElement remove = action["remove"];

        if (remove == null)
        {
            // nothing to remove, always succeeds
            worker = Empty;
            return true;
        }

        string defName = remove.InnerText;

        if (!xenotypeWorker.geneList.Has(defName, out int efficiency))
        {
            // gene list doesn't have gene, so we can't possibly remove it

            if (Settings.devmode)
            {
                XenotypePatchUtils.Message(xenotypeWorker.DefName, $"    Removal of {defName.Colorize(ColoredText.ImpactColor)} would not succeed - not in the genes list".Colorize(Color.gray));
            }

            worker = null;
            return false;
        }

        worker = new ActionWorker_Remove(defName, efficiency);
        return true;
    }

    public override void Apply(XenotypeWorker xenotypeWorker)
    {
        if (Settings.devmode)
        {
            StringBuilder message = new StringBuilder("    Removing ".Colorize(Color.gray))
                .Append(defName.Colorize(ColoredText.ImpactColor));

            if (efficiencyChange != 0)
            {
                message
                    .Append(" (")
                    .Append(XenotypePatchUtils.EfficiencyToColoredString(efficiencyChange))
                    .Append(')');
            }

            XenotypePatchUtils.Message(xenotypeWorker.DefName, message.ToString());
        }

        xenotypeWorker.geneList.Remove(defName);
    }
}
