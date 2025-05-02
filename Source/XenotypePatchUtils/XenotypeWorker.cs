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
        for (int i = 0; i < always.Count; i++)
        {
            if (Settings.devmode)
            {
                XenotypePatchUtils.Message(DefName, $"Doing <always> action {i + 1} of {always.Count}");
            }

            try
            {
                XmlNode action = always[i];

                ActionWorker removeAction = ActionWorker_Remove.Create(action, this);

                if (removeAction == null)
                {
                    if (Settings.devmode)
                    {
                        XenotypePatchUtils.Message(DefName, "Skipping since removal would fail");
                    }

                    continue;
                }

                ActionWorker addAction = ActionWorker_Add.Create(action, this);
                ActionWorker addFirstAction = ActionWorker_AddFirst.Create(action, this);

                if (addAction == null && addFirstAction == null)
                {
                    if (Settings.devmode)
                    {
                        XenotypePatchUtils.Message(DefName, "Skipping since both <add> and <addFirst> would fail");
                    }

                    continue;
                }

                removeAction.Apply(this);
                addAction?.Apply(this);
                addFirstAction?.Apply(this);
            }
            catch (Exception ex)
            {
                XenotypePatchUtils.Error(DefName, $"Error doing <always> action at index {i}: {ex.Message}");
            }
        }
    }

    public void DoMetabolismFixActions()
    {
        if (IsInRange(out int maxIncrease, out int maxDecrease))
        {
            if (Settings.devmode && toFixMetabolism.Count > 0)
            {
                XenotypePatchUtils.Message(DefName, $"Skipping <toFixMetabolism> actions since current metabolic efficiency {geneList.TotalEfficiency.ToStringWithSign()} is within desired range ({desiredEfficiency})");
            }

            return;
        }

        if (Settings.devmode)
        {
            XenotypePatchUtils.Message(DefName, $"Attempting to correct metabolic efficiency to {desiredEfficiency}");
        }

        for (int i = 0; i < toFixMetabolism.Count; i++)
        {
            if (Settings.devmode)
            {
                XenotypePatchUtils.Message(DefName, $"Doing <toFixMetabolism> action {i + 1} of {always.Count}");
            }

            try
            {
                XmlNode action = toFixMetabolism[i];

                ActionWorker removeAction = ActionWorker_Remove.Create(action, this);

                if (removeAction == null)
                {
                    if (Settings.devmode)
                    {
                        XenotypePatchUtils.Message(DefName, "Skipping since removal would fail");
                    }

                    continue;
                }

                ActionWorker addAction = ActionWorker_Add.Create(action, this);
                ActionWorker addFirstAction = ActionWorker_AddFirst.Create(action, this);
                ActionWorker addBestAction = ActionWorker_AddBest.Create(action, this);

                if (addAction == null && addFirstAction == null && addBestAction == null)
                {
                    if (Settings.devmode)
                    {
                        XenotypePatchUtils.Message(DefName, "Skipping since <add>, <addFirst>, and <addBest> would fail");
                    }

                    continue;
                }

                int metabolismImpact = removeAction.efficiencyChange;

                if (addAction != null)
                {
                    metabolismImpact += addAction.efficiencyChange;
                }

                if (addFirstAction != null)
                {
                    metabolismImpact += addFirstAction.efficiencyChange;
                }

                if (addBestAction != null)
                {
                    metabolismImpact += addBestAction.efficiencyChange;
                }

                if (metabolismImpact == 0)
                {
                    if (Settings.devmode)
                    {
                        XenotypePatchUtils.Message(DefName, $"Does not impact metabolic efficiency efficiency");
                    }

                    continue;
                }

                if (metabolismImpact < maxDecrease)
                {
                    if (Settings.devmode)
                    {
                        XenotypePatchUtils.Message(DefName, $"Would decrease metabolic efficiency too much ({metabolismImpact.ToStringWithSign()}, max is {maxDecrease.ToStringWithSign()})");
                    }

                    continue;
                }

                if (metabolismImpact > maxIncrease)
                {
                    if (Settings.devmode)
                    {
                        XenotypePatchUtils.Message(DefName, $"Would increase metabolic efficiency too much ({metabolismImpact.ToStringWithSign()}, max is {maxIncrease.ToStringWithSign()})");
                    }

                    continue;
                }

                removeAction.Apply(this);
                addAction?.Apply(this);
                addFirstAction?.Apply(this);
                addBestAction?.Apply(this);

                if (IsInRange(out maxIncrease, out maxDecrease))
                {
                    if (Settings.devmode)
                    {
                        XenotypePatchUtils.Message(DefName, $"Metabolic efficiency is now in range ({geneList.TotalEfficiency.ToStringWithSign()}), skipping remaining operations");
                    }

                    return;
                }

                if (Settings.devmode)
                {
                    XenotypePatchUtils.Message(DefName, $"New metabolic efficiency = {geneList.TotalEfficiency.ToStringWithSign()}");
                }
            }
            catch (Exception ex)
            {
                XenotypePatchUtils.Error(DefName, $"Error doing <toFixMetabolism> action at index {i}: {ex.Message}");
            }
        }

        if (Settings.devmode)
        {
            XenotypePatchUtils.Message(DefName, $"Final metabolic efficiency is ({geneList.TotalEfficiency.ToStringWithSign()}");
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
