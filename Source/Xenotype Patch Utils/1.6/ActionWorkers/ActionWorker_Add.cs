using System.Text;

namespace XenotypePatchUtils;

public class ActionWorker_Add(string defName, int efficiency) : ActionWorker(efficiency)
{
    private readonly string defName = defName;

    private readonly int efficiency = efficiency;

    public static bool TryCreate(XmlNode action, XenotypeWorker xenotypeWorker, out ActionWorker worker)
    {
        XmlElement add = action["add"];

        if (add == null)
        {
            // nothing to add, always succeeds
            worker = Empty;
            return true;
        }

        string defName = add.InnerText;

        if (xenotypeWorker.geneList.Has(defName, out _))
        {
            // gene list already has gene, we shouldn't add it again

            if (Settings.devmode)
            {
                XenotypePatchUtils.Message(xenotypeWorker.DefName, $"    Addition of {defName.Colorize(ColoredText.ImpactColor)} would not succeed - already in the genes list".Colorize(Color.gray));
            }

            worker = null;
            return false;
        }

        if (!GeneDefResolver.TryGet(add, defName, out int efficiency))
        {
            // GeneDef does not exist, so we shouldn't add it

            if (Settings.devmode)
            {
                XenotypePatchUtils.Message(xenotypeWorker.DefName, $"    Addition of {defName.Colorize(ColoredText.ImpactColor)} would not succeed - GeneDef does not exist".Colorize(Color.gray));
            }

            worker = null;
            return false;
        }

        worker = new ActionWorker_Add(defName, efficiency);
        return true;
    }

    public override void Apply(XenotypeWorker xenotypeWorker)
    {
        if (Settings.devmode)
        {
            StringBuilder message = new StringBuilder("    Adding ".Colorize(Color.gray))
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

        xenotypeWorker.geneList.Add(defName, efficiency);
    }
}
