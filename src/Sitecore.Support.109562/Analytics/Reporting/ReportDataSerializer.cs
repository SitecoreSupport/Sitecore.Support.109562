#region Original code
namespace Sitecore.Support.Analytics.Reporting
{
    using Sitecore.Analytics.Reporting;
    using Sitecore.Analytics.Reports.Filters;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    public class ReportDataSerializer
    {
        private static string Concatenate(IEnumerable<string> values, string separator)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string value in values)
            {
                stringBuilder.Append(value);
                stringBuilder.Append(separator);
            }
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            return stringBuilder.ToString();
        }

        private static void SerializeFilter(XmlWriter writer, FilterEntry filter)
        {
            string value = filter.Id.ToString("N");
            string value2 = Concatenate(filter.Values, "|");
            writer.WriteStartElement("filter");
            writer.WriteAttributeString("id", value);
            writer.WriteAttributeString("values", value2);
            writer.WriteEndElement();
        }

        private static void SerializeParameter(XmlWriter writer, string parameterName, object parameterValue)
        {
            writer.WriteStartElement("parameter");
            writer.WriteAttributeString("name", parameterName);
            if (parameterValue != null)
            {
                NetDataContractSerializer netDataContractSerializer = new NetDataContractSerializer();
                netDataContractSerializer.WriteObject(writer, parameterValue);
            }
            writer.WriteEndElement();
        }

        internal static void SerializeQuery(Stream stream, string source, ReportDataQuery query)
        {
            using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stream, Encoding.UTF8))
            {
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlTextWriter.Indentation = 2;
                xmlTextWriter.WriteStartDocument(true);
                xmlTextWriter.WriteStartElement("request");
                xmlTextWriter.WriteAttributeString("source", source);
                xmlTextWriter.WriteStartElement("query");
                xmlTextWriter.WriteCData(query.Query);
                xmlTextWriter.WriteEndElement();
                if (query.Filters != null && query.Filters.Any())
                {
                    xmlTextWriter.WriteStartElement("filters");
                    foreach (FilterEntry filter in query.Filters)
                    {
                        SerializeFilter(xmlTextWriter, filter);
                    }
                    xmlTextWriter.WriteEndElement();
                }
                if (query.Parameters.Any())
                {
                    xmlTextWriter.WriteStartElement("parameters");
                    foreach (string key in query.Parameters.Keys)
                    {
                        SerializeParameter(xmlTextWriter, key, query.Parameters[key]);
                    }
                    xmlTextWriter.WriteEndElement();
                }
                xmlTextWriter.WriteEndElement();
            }
        }

        internal static string ToXml(string source, ReportDataQuery query)
        {
            byte[] bytes = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                SerializeQuery(memoryStream, source, query);
                bytes = memoryStream.ToArray();
            }
            return Encoding.UTF8.GetString(bytes);
        }
    }
} 
#endregion