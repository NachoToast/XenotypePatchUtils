namespace XenotypePatchUtils;

public static class ActionWorker_AddBest
{
    public static bool TryCreate(XmlNode action, XenotypeWorker xenotypeWorker, out ActionWorker worker)
    {
        List<XmlNode> addBest = Parsers.ParseList(action["addBest"]);

        if (addBest.Count == 0)
        {
            // empty or null list
            worker = ActionWorker.Empty;
            return true;
        }

        int current = xenotypeWorker.geneList.TotalEfficiency;
        float average = xenotypeWorker.desiredEfficiency.Average;

        // bias towards 0
        int target = average >= 0f ? Mathf.FloorToInt(average) : Mathf.CeilToInt(average);

        worker = null;

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
                    XenotypePatchUtils.Message(xenotypeWorker.DefName, $"    <addBest> {i + 1} / {addBest.Count} - {defName.Colorize(ColoredText.ImpactColor)} already in the genes list, skipping all operations".Colorize(Color.gray));
                }

                worker = null;
                return false;
            }

            if (!GeneDefResolver.TryGet(add, defName, out int efficiency))
            {
                // GeneDef does not exist, skip this candidate

                if (Settings.devmode)
                {
                    XenotypePatchUtils.Message(xenotypeWorker.DefName, $"    <addBest> {i + 1} / {addBest.Count} - Addition of {defName.Colorize(ColoredText.ImpactColor)} would not succeed - GeneDef does not exist, going to next node".Colorize(Color.gray));
                }

                continue;
            }

            int score = Mathf.Abs(current + efficiency - target);

            if (Settings.devmode)
            {
                XenotypePatchUtils.Message(xenotypeWorker.DefName, $"    <addBest> {i + 1} / {addBest.Count} - Addition of {defName.Colorize(ColoredText.ImpactColor)} ({XenotypePatchUtils.EfficiencyToString(efficiency)}) would result in a total metabolic efficiency of {XenotypePatchUtils.EfficiencyToString(current + efficiency)} ({score} points away from the average)".Colorize(Color.gray));
            }

            if (score < bestCandidateScore)
            {
                worker = new ActionWorker_Add(defName, efficiency);
                bestCandidateScore = score;
            }
        }

        return worker != null;
    }
}
