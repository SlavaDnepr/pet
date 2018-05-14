using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Monitoring
{
    public class MonitoringManager
    {
        private readonly List<MonitoringJob> jobs;

        public MonitoringManager()
        {
            jobs = 
                new List<MonitoringJob>
                {
                    new MonitoringJob
                    {
                        Title = "Інфанрикс Гекса Социальная аптека",
                        Url = "https://1sa.com.ua/infanriks-geksa-susp-d-in-shpric-por-d-in-1-t.html"
                    },
                    new MonitoringJob
                    {
                        Title = "Ротарикс Социальная аптека",
                        Url = "https://1sa.com.ua/rotariks-susp-d-peror-pr-1-5-ml-1-aplikator-1.html"
                    }
                };
        }

        public void Run()
        {
            foreach (var monitoringJob in jobs)
            {
                //var result = DownloadViaWebClient(monitoringJob);
                var result = DownloadViaAgilityPack(monitoringJob);
                if (!string.IsNullOrEmpty(monitoringJob.LastResult) && monitoringJob.LastResult != result)
                {
                    SendNotification(monitoringJob);
                }
            }
        }

        private string DownloadViaAgilityPack(MonitoringJob monitoringJob)
        {
            var web = new HtmlWeb();
            var doc = web.Load(monitoringJob.Url);
            var outerHtml = doc.DocumentNode.OuterHtml;
            var innerHtml = doc.DocumentNode.InnerHtml;

            return innerHtml;
        }

        private void SendNotification(MonitoringJob monitoringJob)
        {
        }

        private static string DownloadViaWebClient(MonitoringJob monitoringJob)
        {
            using (var client = new WebClient())
            {
                var htmlCode = client.DownloadData(monitoringJob.Url);
                var encoding = GetEncodingFrom(client.ResponseHeaders, Encoding.UTF8);
                return encoding.GetString(htmlCode);
            }
        }

        public static Encoding GetEncodingFrom(NameValueCollection responseHeaders, Encoding defaultEncoding = null)
        {
            if (responseHeaders == null)
                throw new ArgumentNullException("responseHeaders");

            //Note that key lookup is case-insensitive
            var contentType = responseHeaders["Content-Type"];
            if (contentType == null)
                return defaultEncoding;

            var contentTypeParts = contentType.Split(';');
            if (contentTypeParts.Length <= 1)
                return defaultEncoding;

            var charsetPart =
                contentTypeParts.Skip(1).FirstOrDefault(
                    p => p.TrimStart().StartsWith("charset", StringComparison.InvariantCultureIgnoreCase));
            if (charsetPart == null)
                return defaultEncoding;

            var charsetPartParts = charsetPart.Split('=');
            if (charsetPartParts.Length != 2)
                return defaultEncoding;

            var charsetName = charsetPartParts[1].Trim();
            if (charsetName == "")
                return defaultEncoding;

            try
            {
                return Encoding.GetEncoding(charsetName);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException("The server returned data in an unknown encoding: " + charsetName, ex);
            }
        }
    }
}
