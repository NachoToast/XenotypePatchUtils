namespace XenotypePatchUtils;

public class PatchOperation_AutobalanceAll : PatchOperationPathed
{
    public static int NumXenotypesPatched { get; private set; } = 0;

    public static long TimeTaken { get; private set; } = 0L;

    protected override bool ApplyWorker(XmlDocument xml)
    {
        GeneDefResolver.Xml ??= xml;

        Stopwatch watch = Stopwatch.StartNew();

        foreach (XmlNode xenotypeDef in xml.SelectNodes(xpath))
        {
            try
            {
                XenotypeWorker worker = new(xenotypeDef);

                worker.DoAlwaysActions();

                worker.DoMetabolismFixActions();

                NumXenotypesPatched++;
            }
            catch (Exception ex)
            {
                string defName = xenotypeDef["defName"]?.InnerText ?? "Unknown";

                XenotypePatchUtils.Error(defName, $"Error: {ex.Message}");
            }
        }

        watch.Stop();

        TimeTaken = watch.ElapsedMilliseconds;

        return true;
    }
}
