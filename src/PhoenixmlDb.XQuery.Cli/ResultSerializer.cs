using System.Text;
using System.Xml;
using PhoenixmlDb.Core;
using PhoenixmlDb.Xdm.Nodes;

namespace PhoenixmlDb.XQuery.Cli;

/// <summary>
/// Serializes XQuery results to text output.
/// </summary>
internal sealed class ResultSerializer
{
    private readonly DocumentEnvironment _env;
    private readonly TextWriter _output;
    private readonly OutputMethod _method;

    public ResultSerializer(DocumentEnvironment env, TextWriter output, OutputMethod method = OutputMethod.Adaptive)
    {
        _env = env;
        _output = output;
        _method = method;
    }

    /// <summary>
    /// Serializes a single result item.
    /// </summary>
    public void Serialize(object? item)
    {
        switch (item)
        {
            case null:
                break;

            case XdmDocument doc:
                SerializeXmlNode(doc);
                break;

            case XdmElement elem:
                SerializeXmlNode(elem);
                break;

            case XdmAttribute attr:
                if (_method == OutputMethod.Xml)
                    _output.Write($"{attr.LocalName}=\"{EscapeXmlAttribute(attr.Value)}\"");
                else
                    _output.Write(attr.Value);
                break;

            case XdmText text:
                _output.Write(text.Value);
                break;

            case XdmComment comment:
                if (_method == OutputMethod.Xml || _method == OutputMethod.Adaptive)
                    _output.Write($"<!--{comment.Value}-->");
                else
                    _output.Write(comment.Value);
                break;

            case XdmProcessingInstruction pi:
                if (_method == OutputMethod.Xml || _method == OutputMethod.Adaptive)
                    _output.Write($"<?{pi.Target} {pi.Value}?>");
                else
                    _output.Write(pi.Value);
                break;

            case XdmNode node:
                _output.Write(node.StringValue);
                break;

            case object?[] array:
                var first = true;
                foreach (var element in array)
                {
                    if (!first && _method == OutputMethod.Text)
                        _output.Write(' ');
                    Serialize(element);
                    first = false;
                }
                break;

            case IEnumerable<object?> sequence:
                var isFirst = true;
                foreach (var element in sequence)
                {
                    if (!isFirst && _method == OutputMethod.Text)
                        _output.Write(' ');
                    Serialize(element);
                    isFirst = false;
                }
                break;

            default:
                _output.Write(item.ToString());
                break;
        }
    }

    /// <summary>
    /// Writes a newline after a result item, unless the item was empty.
    /// </summary>
    public void WriteNewline()
    {
        _output.WriteLine();
    }

    private void SerializeXmlNode(XdmNode node)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = node is not XdmDocument,
            Encoding = Encoding.UTF8,
            ConformanceLevel = node is XdmDocument ? ConformanceLevel.Document : ConformanceLevel.Fragment
        };

        using var writer = XmlWriter.Create(_output, settings);
        WriteNode(writer, node);
    }

    private void WriteNode(XmlWriter writer, XdmNode node)
    {
        switch (node)
        {
            case XdmDocument doc:
                writer.WriteStartDocument();
                foreach (var childId in doc.Children)
                {
                    var child = _env.GetNode(childId);
                    if (child != null)
                        WriteNode(writer, child);
                }
                writer.WriteEndDocument();
                break;

            case XdmElement elem:
                var ns = _env.ResolveNamespaceUri(elem.Namespace)?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(elem.Prefix))
                    writer.WriteStartElement(elem.Prefix, elem.LocalName, ns);
                else if (!string.IsNullOrEmpty(ns))
                    writer.WriteStartElement(elem.LocalName, ns);
                else
                    writer.WriteStartElement(elem.LocalName);

                // Write namespace declarations
                foreach (var nsDecl in elem.NamespaceDeclarations)
                {
                    var declUri = _env.ResolveNamespaceUri(nsDecl.Namespace)?.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(nsDecl.Prefix))
                        writer.WriteAttributeString("xmlns", declUri);
                    else
                        writer.WriteAttributeString("xmlns", nsDecl.Prefix, null, declUri);
                }

                // Write attributes
                foreach (var attrId in elem.Attributes)
                {
                    if (_env.GetNode(attrId) is XdmAttribute attr)
                    {
                        var attrNs = _env.ResolveNamespaceUri(attr.Namespace)?.ToString() ?? string.Empty;
                        if (!string.IsNullOrEmpty(attr.Prefix))
                            writer.WriteAttributeString(attr.Prefix, attr.LocalName, attrNs, attr.Value);
                        else if (!string.IsNullOrEmpty(attrNs))
                            writer.WriteAttributeString(attr.LocalName, attrNs, attr.Value);
                        else
                            writer.WriteAttributeString(attr.LocalName, attr.Value);
                    }
                }

                // Write children
                foreach (var childId in elem.Children)
                {
                    var child = _env.GetNode(childId);
                    if (child != null)
                        WriteNode(writer, child);
                }

                writer.WriteEndElement();
                break;

            case XdmText text:
                writer.WriteString(text.Value);
                break;

            case XdmComment comment:
                writer.WriteComment(comment.Value);
                break;

            case XdmProcessingInstruction pi:
                writer.WriteProcessingInstruction(pi.Target, pi.Value);
                break;
        }
    }

    private static string EscapeXmlAttribute(string value)
    {
        return value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);
    }
}

/// <summary>
/// Output serialization method.
/// </summary>
internal enum OutputMethod
{
    /// <summary>
    /// Automatically choose XML for nodes, text for atomic values.
    /// </summary>
    Adaptive,

    /// <summary>
    /// Always serialize as XML.
    /// </summary>
    Xml,

    /// <summary>
    /// Always serialize as text (string values only).
    /// </summary>
    Text
}
