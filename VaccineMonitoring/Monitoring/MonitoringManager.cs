using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Com.CloudRail.SI;
using Com.CloudRail.SI.ServiceCode.Commands.CodeRedirect;
using Com.CloudRail.SI.Services;
using HtmlAgilityPack;
using WatiN.Core;

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
                    },
                    new MonitoringJob
                    {
                        Title = "Инфанрикс Гекса Aптека 24",
                        Url = "https://www.apteka24.ua/infanriks-geksa-fl-1d-n1-shprits-2igla/"
                    },
                    new MonitoringJob
                    {
                        Title = "Ротарикс Aптека 24",
                        Url = "https://www.apteka24.ua/rotariks-n1/"
                    }
                };
        }

        public void Run()
        {
            foreach (var monitoringJob in jobs)
            {
                var result = string.Empty;
                var thread = new Thread(() => 
                {
                    Settings.Instance.MakeNewIeInstanceVisible = false;
                    var ie = new IE(monitoringJob.Url) { Visible = false };
                    ie.WaitForComplete();
                    result = ie.Html;
                    ie.Close();
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(monitoringJob.LastResult) && monitoringJob.LastResult != result)
                    SendNotification(monitoringJob);
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
            //// https://cloudrail.com/apps/5af98a878de7127c203f08f0
            //CloudRail.AppKey = "Monitoring";
            //var viber = new Viber(null, "", "", "");
            //viber.SendMessage("+380989897825", monitoringJob.Title);

            
            var client = 
                new SmtpClient
                {
                    Port = 587,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("slavikmaliy@gmail.com", "krasota."),
                    Host = "smtp.gmail.com",
                    EnableSsl = true
                };
            var mail = 
                new MailMessage("slavikmaliy@gmail.com>", "maliy_sl@ua.fm")
                {
                    Subject = "Что то поменялось",
                    Body = monitoringJob.Title + "\n" + monitoringJob.Url
                };

            client.Send(mail);
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
