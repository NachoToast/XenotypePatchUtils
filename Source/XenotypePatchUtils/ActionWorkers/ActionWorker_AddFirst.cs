namespace XenotypePatchUtils;

public static class ActionWorker_AddFirst
{
    public static bool TryCreate(XmlNode action, XenotypeWorker xenotypeWorker, out ActionWorker worker)
    {
        List<XmlNode> addFirst = Parsers.ParseList(action["addFirst"]);

        if (addFirst.Count == 0)
        {
            // empty or null list
            worker = ActionWorker.Empty;
            return true;
        }

        worker = null;

        // we have to iterate through every child, even if one is able to be added, since a
        // subsequent one might already be in the gene list (in which case we should not add any)
        for (int i = 0; i < addFirst.Count; i++)
        {
            XmlNode add = addFirst[i];

            string defName = add.InnerText;

            if (xenotypeWorker.geneList.Has(defName, out _))
            {
                // gene list already has gene, don't add any candidates

                if (Settings.devmode)
                {
                    XenotypePatchUtils.Message(xenotypeWorker.DefName, $"    <addFirst> {i + 1} / {addFirst.Count} - {defName.Colorize(ColoredText.ImpactColor)} already in the genes list, skipping all operations".Colorize(Color.gray));
                }

                worker = null;
                return false;
            }

            if (!GeneDefResolver.TryGet(add, defName, out int efficiency))
            {
                // GeneDef does not exist, skip this candidate

                if (Settings.devmode)
                {
                    XenotypePatchUtils.Message(xenotypeWorker.DefName, $"    <addFirst> {i + 1} / {addFirst.Count} - Addition of {defName.Colorize(ColoredText.ImpactColor)} would not succeed - GeneDef does not exist".Colorize(Color.gray));
                }

                continue;
            }

            worker ??= new ActionWorker_Add(defName, efficiency);
        }

        // might be null here, i.e. no mods were active, in which case operation should be seen as a
        // fail since there definitely were elements defined
        return worker != null;
    }
}
