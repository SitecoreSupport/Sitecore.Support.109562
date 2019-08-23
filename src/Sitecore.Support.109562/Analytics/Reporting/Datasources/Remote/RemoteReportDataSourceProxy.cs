namespace Sitecore.Support.Analytics.Reporting.Datasources.Remote
{
    using Sitecore.Analytics.Commons;
    using Sitecore.Analytics.Reporting;
    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Sitecore.Xml;
    using System;
    using System.Data;
    using System.IO;
    using System.Net;
    using System.Xml;

    public class RemoteReportDataSourceProxy : Sitecore.Analytics.Reporting.Datasources.Remote.RemoteReportDataSourceProxy
    {
        private string GetPath(string xpath)
        {
            XmlNode configNode = Factory.GetConfigNode(xpath);
            Assert.IsNotNull(configNode, "path is not configured");
            string value = XmlUtil.GetValue(configNode);
            Assert.IsNotNullOrEmpty(value, "path is not configured");
            return value;
        }

        public override DataTable GetData(ReportDataQuery query)
        {
            Assert.ArgumentNotNull(query, "query");
            WebTransportFactoryBase webTransportFactoryBase = Factory.CreateObject("httpTransportFactory", true) as WebTransportFactoryBase;
            Assert.IsNotNull(webTransportFactoryBase, "httpTransportFactory is required");
            string path = GetPath("reporting/remote/paths/Reporting");
            WebRequest webRequest = webTransportFactoryBase.CreateWebRequest(path);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/xml";
            #region Added code
            webRequest.Timeout = 12000000; 
            #endregion
            using (StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream()))
            {
                string value = Sitecore.Support.Analytics.Reporting.ReportDataSerializer.ToXml(RemoteDataSourceName, query);
                streamWriter.Write(value);
            }
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                object obj = Sitecore.Analytics.Reporting.ReportDataSerializer.DeserializeResponse(webResponse.GetResponseStream());
                if (obj is Exception)
                {
                    throw obj as Exception;
                }
                return (DataTable)obj;
            }
        }
    }
}