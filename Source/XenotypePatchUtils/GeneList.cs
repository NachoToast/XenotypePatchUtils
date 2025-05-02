namespace XenotypePatchUtils;

public class GeneList
{
    private readonly XmlElement xml;

    private readonly Dictionary<string, Tuple<XmlNode, int>> genes;

    public int TotalEfficiency { get; private set; } = 0;

    private string DefName => xml.ParentNode["defName"]?.InnerText ?? "Unknown";

    public GeneList(XmlNode xenotypeDef)
    {
        xml = xenotypeDef["genes"];

        if (xml == null)
        {
            throw new Exception("Missing <genes> element");
        }

        genes = [];

        XmlNodeList items = xml.ChildNodes;

        for (int i = 0; i < xml.ChildNodes.Count; i++)
        {
            try
            {
                XmlNode geneReference = items[i];

                string defName = geneReference.InnerText;

                if (string.IsNullOrEmpty(defName))
                {
                    throw new Exception("Node has no content");
                }

                if (GeneDefResolver.TryGet(geneReference, defName, out int efficiency))
                {
                    genes.Add(defName, new Tuple<XmlNode, int>(geneReference, efficiency));
                    TotalEfficiency += efficiency;
                }
            }
            catch (Exception ex)
            {
                XenotypePatchUtils.Error(DefName, $"Error resolving <li> node in <genes> at index {i}: {ex.Message}");
            }
        }

        if (Settings.devmode)
        {
            XenotypePatchUtils.Message(DefName, $"Resolved {genes.Count} / {items.Count} <genes> (total efficiency = {TotalEfficiency.ToStringWithSign()})");
        }
    }

    public bool Has(string defName, out int efficiency)
    {
        if (genes.TryGetValue(defName, out Tuple<XmlNode, int> geneInfo))
        {
            efficiency = geneInfo.Item2;
            return true;
        }

        efficiency = 0;
        return false;
    }

    public bool TestRemove(string defName, ref int efficiency)
    {
        if (genes.TryGetValue(defName, out Tuple<XmlNode, int> geneInfo))
        {
            efficiency = geneInfo.Item2;
            return true;
        }

        return genes.ContainsKey(defName);
    }

    public void Remove(string defName)
    {
        if (!genes.Remove(defName, out Tuple<XmlNode, int> geneInfo))
        {
            throw new Exception($"Tried to remove {defName} from the genes list, which does not currently contain {defName}");
        }

        if (xml.RemoveChild(geneInfo.Item1) == null)
        {
            throw new Exception($"Failed to remove <li> node from <genes> (text = {defName})");
        }

        TotalEfficiency -= geneInfo.Item2;
    }

    public bool TestAdd(string defName)
    {
        return !genes.ContainsKey(defName);
    }

    public void Add(string defName, int efficiency)
    {
        if (genes.ContainsKey(defName))
        {
            throw new Exception($"Tried to add {defName} to the genes list, which already contains {defName}");
        }

        XmlElement geneReference = xml.OwnerDocument.CreateElement("li");
        geneReference.InnerText = defName;

        genes.Add(defName, new Tuple<XmlNode, int>(geneReference, efficiency));

        if (xml.AppendChild(geneReference) == null)
        {
            throw new Exception($"Failed to append <li> node to <genes> (text = {defName})");
        }

        TotalEfficiency += efficiency;
    }
}
