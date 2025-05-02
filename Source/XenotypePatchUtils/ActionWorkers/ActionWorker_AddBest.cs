namespace XenotypePatchUtils;

public static class ActionWorker_AddBest
{
    public static ActionWorker Create(
        XmlNode action,
        XenotypeWorker xenotypeWorker)
    {
        List<XmlNode> addBest = Parsers.ParseList(action["addBest"]);

        if (addBest.Count == 0)
        {
            // empty or null list
            return ActionWorker.Empty;
        }

        int current = xenotypeWorker.geneList.TotalEfficiency;
        float average = xenotypeWorker.desiredEfficiency.Average;

        // bias towards 0
        int target = average >= 0f ? Mathf.FloorToInt(average) : Mathf.CeilToInt(average);

        ActionWorker bestCandidateSoFar = null;

        // score represents how many ME points the gene brings the current value AWAY the average,
        // so lower = better
        int bestCandidateScore = int.MaxValue;

        for (int i = 0; i < addBest.Count; i++)
        {
            XmlNode add = addBest[i];

            string defName = add.InnerText;

            if (xenotypeWorker.geneList.Has(defName, out _))
            {
                // gene list already has gene, don't add any candidates

                if (Settings.devmode)
                {
                    XenotypePatchUtils.Message(xenotypeWorker.DefName, $"<addBest>[{i + 1}/{addBest.Count}] {defName} already in the genes list, skipping all operations");
                }

                return null;
            }

            if (!GeneDefResolver.TryGet(add, defName, out int efficiency))
            {
                // GeneDef does not exist, skip this candidate

                if (Settings.devmode)
                {
                    XenotypePatchUtils.Message(xenotypeWorker.DefName, $"<addBest>[{i + 1}/{addBest.Count}] Addition of {defName} would not succeed - GeneDef does not exist, going to next node");
                }

                continue;
            }

            int score = Mathf.Abs(current + efficiency - target);

            if (Settings.devmode)
            {
                XenotypePatchUtils.Message(xenotypeWorker.DefName, $"<addBest>[{i + 1}/{addBest.Count}] Addition of {defName} ({efficiency.ToStringWithSign()}) would result in a total metabolic efficiency of {(current + efficiency).ToStringWithSign()} ({score} points away from the average)");
            }

            if (score < bestCandidateScore)
            {
                bestCandidateSoFar = new ActionWorker_Add(defName, efficiency);
                bestCandidateScore = score;
            }
        }

        return bestCandidateSoFar;
    }
}
