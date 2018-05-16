using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using HtmlAgilityPack;
using log4net;
using WatiN.Core;

namespace VaccineMonitoring.Console
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
                        Title = "Infanrix Gexa 1SA",
                        Url = "https://1sa.com.ua/infanriks-geksa-susp-d-in-shpric-por-d-in-1-t.html",
                        Warehouse = Warehouse.SA
                    },
                    new MonitoringJob
                    {
                        Title = "Rotarix 1SA",
                        Url = "https://1sa.com.ua/rotariks-susp-d-peror-pr-1-5-ml-1-aplikator-1.html",
                        Warehouse = Warehouse.SA
                    },
                    new MonitoringJob
                    {
                        Title = "Infanrix Gexa Apteka24",
                        Url = "https://www.apteka24.ua/infanriks-geksa-fl-1d-n1-shprits-2igla/",
                        Warehouse = Warehouse.Apteka24
                    },
                    new MonitoringJob
                    {
                        Title = "Rotarix Apteka24",
                        Url = "https://www.apteka24.ua/rotariks-n1/",
                        Warehouse = Warehouse.Apteka24
                    }
                };
        }

        public void Run()
        {
            foreach (var monitoringJob in jobs)
            {
                try
                {

                    var result = string.Empty;
                    var thread = new Thread(
                        () =>
                        {
                            try
                            {
                                Settings.Instance.MakeNewIeInstanceVisible = false;
                                var ie = new IE(monitoringJob.Url) { Visible = false };
                                ie.WaitForComplete();
                                if (monitoringJob.Warehouse == Warehouse.Apteka24)
                                {
                                    var productcarts = ie.Body.Divs.Where(arg => arg.ClassName == "productcart");
                                    result = productcarts.FirstOrDefault()?.InnerHtml;
                                }
                                else if (monitoringJob.Warehouse == Warehouse.SA)
                                {
                                    var productessentials = ie.Body.Divs.Where(arg => arg.ClassName == "product-essential");
                                    result = productessentials.FirstOrDefault()?.InnerHtml;
                                }
                                else
                                    result = ie.Html;
                                ie.Close();
                            }
                            catch (Exception exception)
                            {
                                LogManager.GetLogger("MonitoringLogger").Error("Error during ie emulation" + monitoringJob.Title, exception);
                            }
                        });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();

                    if (!string.IsNullOrEmpty(result))
                    {
                        result = ModifyResult(monitoringJob, result);

                        if (!string.IsNullOrEmpty(monitoringJob.LastResult) && monitoringJob.LastResult != result)
                        {
                            LogManager.GetLogger("MonitoringLogger").Info("Result was changed for " + monitoringJob.Title);
                            LogManager.GetLogger("MonitoringLogger").Info("Notification will be sent");
                            for (var i = 0; i <= result.Length; i++)
                            {
                                if (result[i] != monitoringJob.LastResult[i])
                                {
                                    LogManager.GetLogger("MonitoringLogger").Info("Diff " + result[i - 1].ToString() + result[i].ToString() + result[i + 1].ToString() + result[i + 2].ToString());
                                    LogManager.GetLogger("MonitoringLogger").Info("Diff " + monitoringJob.LastResult[i - 1].ToString() + monitoringJob.LastResult[i].ToString() + monitoringJob.LastResult[i + 1].ToString() + monitoringJob.LastResult[i + 2].ToString());
                                    break;
                                }
                            }

                            SendNotification(monitoringJob);
                        }
                        else
                            LogManager.GetLogger("MonitoringLogger").Info("Nothing was changed for " + monitoringJob.Title);

                        monitoringJob.LastResult = result;
                    }
                }
                catch (Exception exception)
                {
                    LogManager.GetLogger("MonitoringLogger").Error("Error during update" + monitoringJob.Title, exception);
                }
            }
        }

        private static string ModifyResult(MonitoringJob monitoringJob, string result)
        {
            if (monitoringJob.Warehouse == Warehouse.SA)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(result);
                var specificNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'product-info-tabs')]");
                specificNode.Remove();
                result = doc.DocumentNode.InnerHtml;

                //for (var i = 0; i < 3; i++)
                //{
                //    var startIndex = result.IndexOf("cdz-nav-tab");
                //    if (startIndex != -1)
                //        result = result.Remove(startIndex, 14);
                //}
            }

            if (monitoringJob.Warehouse == Warehouse.Apteka24)
            {
                for (var i = 0; i < 3; i++)
                {
                    var startIndex = result.IndexOf("SERVER_TIME");
                    if (startIndex != -1)
                        result = result.Remove(startIndex, 26);
                }

                for (var i = 0; i < 3; i++)
                {
                    var startIndex = result.IndexOf("href=\"#rot");
                    if (startIndex != -1)
                        result = result.Remove(startIndex, 25);
                }

                for (var i = 0; i < 3; i++)
                {
                    var startIndex = result.IndexOf("id=\"rot");
                    if (startIndex != -1)
                        result = result.Remove(startIndex, 22);
                }
            }

            return result;
        }

        private void SendNotification(MonitoringJob monitoringJob)
        {
            try
            {
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
                    new MailMessage("slavikmaliy@gmail.com", "maliy_sl@ua.fm")
                    {
                        Subject = monitoringJob.Title,
                        Body = monitoringJob.Title + "\n" + monitoringJob.Url
                    };

                client.Send(mail);
                LogManager.GetLogger("MonitoringLogger").Info("Notification was sent for " + monitoringJob.Title);
            }
            catch (Exception exception)
            {
                LogManager.GetLogger("MonitoringLogger").Error("Error during notification" + monitoringJob.Title, exception);
            }
        }
    }
}
