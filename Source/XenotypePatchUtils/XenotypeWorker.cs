namespace XenotypePatchUtils;

public partial class XenotypeWorker
{
    private readonly XmlNode xenotypeDef;
    private readonly XmlNode modExtension;

    public readonly IntRange desiredEfficiency;

    private readonly List<XmlNode> always;
    private readonly List<XmlNode> toFixMetabolism;

    public readonly GeneList geneList;

    public string DefName => xenotypeDef["defName"]?.InnerText ?? "Unknown";

    public XenotypeWorker(XmlNode xenotypeDef)
    {
        this.xenotypeDef = Parsers.ParseXenotypeDef(xenotypeDef);

        modExtension = Parsers.ParseModExtension(xenotypeDef);

        desiredEfficiency = Parsers.ParseDesiredEfficiency(modExtension);

        always = Parsers.ParseList(modExtension["always"]);

        toFixMetabolism = Parsers.ParseList(modExtension["toFixMetabolism"]);

        geneList = new GeneList(xenotypeDef);
    }

    public void DoAlwaysActions()
    {
        string alwaysStr = "<always>".Colorize(new Color(1f, 0.84f, 0f));

        for (int i = 0; i < always.Count; i++)
        {
            if (Settings.devmode)
            {
                XenotypePatchUtils.Message(DefName, $"{alwaysStr} {i + 1} / {always.Count}");
            }

            try
            {
                XmlNode action = always[i];

                if (!ActionWorker_Remove.TryCreate(action, this, out ActionWorker removeAction) ||
                    !ActionWorker_Add.TryCreate(action, this, out ActionWorker addAction) ||
                    !ActionWorker_AddFirst.TryCreate(action, this, out ActionWorker addFirstAction))
                {
                    continue;
                }

                removeAction.Apply(this);
                addAction.Apply(this);
                addFirstAction.Apply(this);
            }
            catch (Exception ex)
            {
                XenotypePatchUtils.Error(DefName, $"Error doing <always> action at index {i}: {ex.Message}");
            }
        }
    }

    public void DoMetabolismFixActions()
    {
        string toFixStr = "<toFixMetabolism>".Colorize(new Color(1f, 0.84f, 0f));

        if (IsInRange(out int maxIncrease, out int maxDecrease))
        {
            if (Settings.devmode && toFixMetabolism.Count > 0)
            {
                XenotypePatchUtils.Message(DefName, $"Skipping {toFixStr} actions since current metabolic efficiency {XenotypePatchUtils.EfficiencyToString(geneList.TotalEfficiency)} is within desired range ({XenotypePatchUtils.EfficiencyToString(desiredEfficiency)})");
            }

            return;
        }

        if (Settings.devmode)
        {
            XenotypePatchUtils.Message(DefName, $"Attempting to correct metabolic efficiency to {XenotypePatchUtils.EfficiencyToString(desiredEfficiency)} (currently is {XenotypePatchUtils.EfficiencyToString(geneList.TotalEfficiency)})");
        }

        for (int i = 0; i < toFixMetabolism.Count; i++)
        {
            if (Settings.devmode)
            {
                XenotypePatchUtils.Message(DefName, $"{toFixStr} {i + 1} / {toFixMetabolism.Count}");
            }

            try
            {
                XmlNode action = toFixMetabolism[i];

                if (!ActionWorker_Remove.TryCreate(action, this, out ActionWorker removeAction) ||
                    !ActionWorker_Add.TryCreate(action, this, out ActionWorker addAction) ||
                    !ActionWorker_AddFirst.TryCreate(action, this, out ActionWorker addFirstAction) ||
                    !ActionWorker_AddBest.TryCreate(action, this, out ActionWorker addBestAction))
                {
                    continue;
                }

                int metabolismImpact = removeAction.efficiencyChange +
                    addAction.efficiencyChange +
                    addFirstAction.efficiencyChange +
                    addBestAction.efficiencyChange;

                if (metabolismImpact == 0)
                {
                    if (Settings.devmode)
                    {
                        XenotypePatchUtils.Message(DefName, "    Does not impact metabolic efficiency efficiency");
                    }

                    continue;
                }

                if (metabolismImpact < maxDecrease)
                {
                    if (Settings.devmode)
                    {
                        XenotypePatchUtils.Message(DefName, $"    Would decrease metabolic efficiency too much ({XenotypePatchUtils.EfficiencyToColoredString(metabolismImpact)}, max is {XenotypePatchUtils.EfficiencyToString(maxDecrease)})");
                    }

                    continue;
                }

                if (metabolismImpact > maxIncrease)
                {
                    if (Settings.devmode)
                    {
                        XenotypePatchUtils.Message(DefName, $"    Would increase metabolic efficiency too much ({XenotypePatchUtils.EfficiencyToColoredString(metabolismImpact)}, max is {XenotypePatchUtils.EfficiencyToString(maxIncrease)})");
                    }

                    continue;
                }

                removeAction.Apply(this);
                addAction.Apply(this);
                addFirstAction.Apply(this);
                addBestAction.Apply(this);

                if (IsInRange(out maxIncrease, out maxDecrease))
                {
                    if (Settings.devmode)
                    {
                        XenotypePatchUtils.Message(DefName, $"    Metabolic efficiency ({XenotypePatchUtils.EfficiencyToString(geneList.TotalEfficiency)}) {"is now in range".Colorize(new Color(0.56f, 0.93f, 0.56f))} ({XenotypePatchUtils.EfficiencyToString(desiredEfficiency)}), skipping remaining operations");
                    }

                    return;
                }

                if (Settings.devmode)
                {
                    XenotypePatchUtils.Message(DefName, $"    Total metabolic efficiency is now {XenotypePatchUtils.EfficiencyToString(geneList.TotalEfficiency)}".Colorize(Color.gray));
                }
            }
            catch (Exception ex)
            {
                XenotypePatchUtils.Error(DefName, $"Error doing <toFixMetabolism> action at index {i}: {ex.Message}");
            }
        }

        if (Settings.devmode)
        {
            XenotypePatchUtils.Message(DefName, $"Final metabolic efficiency is {XenotypePatchUtils.EfficiencyToString(geneList.TotalEfficiency)} ({"not in target range".Colorize(new Color(0.94f, 0.5f, 0.5f))})");
        }
    }

    private bool IsInRange(out int maxIncrease, out int maxDecrease)
    {
        int min = desiredEfficiency.min;
        int max = desiredEfficiency.max;
        int current = geneList.TotalEfficiency;

        if (current < min)
        {
            maxIncrease = max - current;
            maxDecrease = 0;
            return false;
        }

        if (current > max)
        {
            maxIncrease = 0;
            maxDecrease = min - current;
            return false;
        }

        maxIncrease = 0;
        maxDecrease = 0;
        return true;
    }
}
