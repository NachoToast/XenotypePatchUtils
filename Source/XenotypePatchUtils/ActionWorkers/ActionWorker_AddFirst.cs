namespace XenotypePatchUtils;

public static class ActionWorker_AddFirst
{
    public static ActionWorker Create(XmlNode action, XenotypeWorker xenotypeWorker)
    {
        List<XmlNode> addFirst = Parsers.ParseList(action["addFirst"]);

        if (addFirst.Count == 0)
        {
            // empty or null list
            return ActionWorker.Empty;
        }

        ActionWorker bestCandidateSoFar = null;

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
                    XenotypePatchUtils.Message(xenotypeWorker.DefName, $"<addFirst>[{i + 1}/{addFirst.Count}] {defName} already in the genes list, skipping all operations");
                }

                return null;
            }

            if (!GeneDefResolver.TryGet(add, defName, out int efficiency))
            {
                // GeneDef does not exist, skip this candidate

                if (Settings.devmode)
                {
                    XenotypePatchUtils.Message(xenotypeWorker.DefName, $"<addFirst>[{i + 1}/{addFirst.Count}] Addition of {defName} would not succeed - GeneDef does not exist, going to next node");
                }

                continue;
            }

            bestCandidateSoFar ??= new ActionWorker_Add(defName, efficiency);
        }

        // might be null here, i.e. no mods were active, in which case operation should be seen as a
        // fail since there definitely were elements defined
        return bestCandidateSoFar;
    }
}
