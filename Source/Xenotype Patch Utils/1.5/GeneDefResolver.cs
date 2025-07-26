namespace XenotypePatchUtils;

/// <summary>
/// Helper class for seeing if a <see cref="GeneDef"/> exists.
/// </summary>
public static class GeneDefResolver
{
    /// <summary>
    /// Previously-searched GeneDefs mapped to their metabolic efficiency.
    /// </summary>
    private static readonly Dictionary<string, int> cachedGenes = [];

    /// <summary>
    /// Previously-searched GeneDefs that were not found.
    /// </summary>
    private static readonly HashSet<string> missingGenes = [];

    public static XmlDocument Xml { get; set; }

    public static void Clear(out int cachedGeneCount, out int missingGeneCount)
    {
#if DEBUG
        Log.Message($"CACHED GENES:\n" + string.Join("\n", cachedGenes.Select(x => $"{x.Key}({x.Value})")));
        Log.Message($"MISSING GENES:\n" + string.Join("\n", missingGenes));
#endif

        cachedGeneCount = cachedGenes.Count;
        missingGeneCount = missingGenes.Count;

        cachedGenes.Clear();
        missingGenes.Clear();

        Xml = null;
    }

    /// <summary>
    /// Attempts to find a GeneDef and it's metabolic efficiency via defName.
    /// </summary>
    /// <remarks>
    /// Throws an <see cref="Exception"/> if the GeneDef cannot be found when it should have been
    /// (e.g. not suppressed by a <see cref="MayRequireAttribute"/>).
    /// </remarks>
    /// <returns>
    /// Returns <see langword="true"/> or <see langword="false"/> depending on whether the GeneDef
    /// exists or not.
    /// </returns>
    public static bool TryGet(XmlNode geneReference, string defName, out int efficiency)
    {
        if (cachedGenes.TryGetValue(defName, out efficiency))
        {
            // cached :)
            return true;
        }

        if (missingGenes.Contains(defName))
        {
            // cached :/
            return false;
        }

        if (TryResolveInternal(defName, out efficiency))
        {
            // found :)
            cachedGenes.SetOrAdd(defName, efficiency);

            return true;
        }

        // not found :(
        missingGenes.Add(defName);

        if (MustResolve(geneReference))
        {
            throw new Exception($"Unable to resolve GeneDef named \"{defName}\"");
        }

        return false;
    }

    /// <summary>
    /// Whether an exception should be thrown for the given failed <see cref="GeneDef"/> reference node.
    /// </summary>
    /// <remarks>
    /// This is based on the values of the <see cref="MayRequireAttribute"/> and <see cref="MayRequireAnyOfAttribute"/>.
    /// </remarks>
    private static bool MustResolve(XmlNode node)
    {
        if (node is not XmlElement element)
        {
            // A non-element? In the XML file? How queer! - I must inquire about this further with
            // my supervisor post-haste!
            return true;
        }

        string mayRequire = element.GetAttribute("MayRequire");

        // MayRequire = ALL must be active, so attribute will suppress errors when any are not active.
        if (!string.IsNullOrEmpty(mayRequire) && mayRequire.Split(',').Any(x => !ModsConfig.IsActive(x)))
        {
            return false;
        }

        string mayRequireAnyOf = element.GetAttribute("MayRequireAnyOf");

        // MayRequireAnyOf = ANY must be active, so attribute will suppress errors when none are active.
        if (!string.IsNullOrEmpty(mayRequireAnyOf) && mayRequireAnyOf.Split(',').All(x => !ModsConfig.IsActive(x)))
        {
            return false;
        }

        return true;
    }

    private static bool TryResolveGeneDef(string key, string value, out int efficiency)
    {
        XmlNode def = Xml.SelectSingleNode($"Defs/GeneDef[{key}=\"{value}\"]");

        if (def == null)
        {
            efficiency = 0;
            return false;
        }

        string rawEfficiency = def["biostatMet"]?.InnerText;

        if (!string.IsNullOrEmpty(rawEfficiency))
        {
            efficiency = int.Parse(rawEfficiency);
            return true;
        }

        if (def is XmlElement el && !el.GetAttribute("ParentName").NullOrEmpty())
        {
            return TryResolveGeneDef("@Name", el.GetAttribute("ParentName"), out efficiency);
        }

        efficiency = 0;
        return true;
    }

    private static bool TryResolveGeneTemplateDef(string key, string value, string subKey, out int efficiency)
    {
        XmlNode def = Xml.SelectSingleNode($"Defs/GeneTemplateDef[{key}=\"{value}\"]");

        if (def == null)
        {
            efficiency = 0;
            return false;
        }

        List<XmlNode> overrides = Parsers.ParseList(def["chemicalBiostatOverrides"]);

        foreach (XmlNode overrideNode in overrides)
        {
            string chemical = overrideNode["chemical"]?.InnerText;

            if (chemical != subKey)
            {
                continue;
            }

            string biostatMet = overrideNode["biostatMet"]?.InnerText;

            if (string.IsNullOrEmpty(biostatMet))
            {
                continue;
            }

            efficiency = int.Parse(biostatMet);
            return true;
        }

        string rawEfficiency = def["biostatMet"]?.InnerText;

        if (!string.IsNullOrEmpty(rawEfficiency))
        {
            efficiency = int.Parse(rawEfficiency);
            return true;
        }

        if (def is XmlElement el && !el.GetAttribute("ParentName").NullOrEmpty())
        {
            return TryResolveGeneTemplateDef("@Name", el.GetAttribute("ParentName"), subKey, out efficiency);
        }

        efficiency = 0;
        return true;
    }

    private static bool TryResolveInternal(string defName, out int efficiency)
    {
        if (TryResolveGeneDef("defName", defName, out efficiency))
        {
            return true;
        }

        int underscoreIndex = defName.IndexOf('_');

        if (underscoreIndex < 1)
        {
            return false;
        }

        string actualDefName = defName.Substring(0, underscoreIndex);
        string subKey = defName.Substring(underscoreIndex + 1);

        return TryResolveGeneTemplateDef("defName", actualDefName, subKey, out efficiency);
    }
}
