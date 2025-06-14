namespace XenotypePatchUtils;

public class ActionWorker_AddAll(List<ActionWorker_Add> subActions) : ActionWorker(subActions.Sum(x => x.efficiencyChange))
{
    private readonly List<ActionWorker_Add> subActions = subActions;

    public static bool TryCreate(XmlNode action, XenotypeWorker xenotypeWorker, out ActionWorker worker)
    {
        List<XmlNode> addAll = Parsers.ParseList(action["addAll"]);

        if (addAll.Count == 0)
        {
            // empty or null list
            worker = Empty;
            return true;
        }

        List<ActionWorker_Add> workers = [];

        for (int i = 0; i < addAll.Count; i++)
        {
            XmlNode add = addAll[i];

            string defName = add.InnerText;

            if (xenotypeWorker.geneList.Has(defName, out _))
            {
                // gene list already has gene, don't add any

                if (Settings.devmode)
                {
                    XenotypePatchUtils.Message(xenotypeWorker.DefName, $"    <addAll> {i + 1} / {addAll.Count} - {defName.Colorize(ColoredText.ImpactColor)} already in the genes list, skipping all operations".Colorize(Color.gray));
                }

                worker = null;
                return false;
            }

            if (!GeneDefResolver.TryGet(add, defName, out int efficiency))
            {
                // GeneDef does not exist, skip this candidate

                if (Settings.devmode)
                {
                    XenotypePatchUtils.Message(xenotypeWorker.DefName, $"    <addAll> {i + 1} / {addAll.Count} - Addition of {defName.Colorize(ColoredText.ImpactColor)} would not succeed - GeneDef does not exist".Colorize(Color.gray));
                }

                continue;
            }

            workers.Add(new ActionWorker_Add(defName, efficiency));
        }

        worker = new ActionWorker_AddAll(workers);
        return true;
    }

    public override void Apply(XenotypeWorker xenotypeWorker)
    {
        foreach (ActionWorker_Add action in subActions)
        {
            action.Apply(xenotypeWorker);
        }
    }
}
