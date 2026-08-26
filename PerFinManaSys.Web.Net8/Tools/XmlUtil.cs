using System.Xml;

namespace PerFinManaSys.Web.Net8.Tools;

/// <summary>
/// XML 辅助（配置读写，net8 兼容）。
/// </summary>
public static class XmlUtil
{
    public static XmlDocument Load(string path)
    {
        var doc = new XmlDocument();
        doc.Load(path);
        return doc;
    }

    public static string? ReadNode(XmlDocument doc, string xpath)
    {
        return doc.SelectSingleNode(xpath)?.InnerText;
    }
}
