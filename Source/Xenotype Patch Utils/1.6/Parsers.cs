namespace XenotypePatchUtils;

public static class Parsers
{
    private static readonly string ModExtensionPath = $"li[@Class=\"XenotypePatchUtils.{nameof(PatchMePlease)}\"]";

    public static XmlNode ParseXenotypeDef(XmlNode xenotypeDef)
    {
        if (xenotypeDef == null)
        {
            throw new Exception("Missing <XenotypeDef> element, this should never happen!");
        }

        return xenotypeDef;
    }

    public static XmlNode ParseModExtension(XmlNode xenotypeDef)
    {
        XmlNode modExtensionList = xenotypeDef["modExtensions"];

        if (modExtensionList == null)
        {
            throw new Exception("Missing <modExtensions> element, this should never happen!");
        }

        XmlNode modExtension = modExtensionList.SelectSingleNode(ModExtensionPath);

        if (modExtension == null)
        {
            throw new Exception("Missing mod extension, this should never happen!");
        }

        return modExtension;
    }

    public static IntRange ParseDesiredEfficiency(XmlNode modExtension)
    {
        string rawValue = modExtension["desiredEfficiencyRange"]?.InnerText;

        if (string.IsNullOrEmpty(rawValue))
        {
            return new IntRange(int.MinValue, int.MaxValue);
        }

        return IntRange.FromString(rawValue);
    }

    public static List<XmlNode> ParseList(XmlElement root)
    {
        XmlNodeList children = root?.ChildNodes;

        if (children == null)
        {
            return [];
        }

        List<XmlNode> items = [];

        foreach (XmlNode item in children)
        {
            items.Add(item);
        }

        return items;
    }
}
