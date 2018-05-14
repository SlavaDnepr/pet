using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
                using (var client = new WebClient())
                {
                    var htmlCode = client.DownloadString(monitoringJob.Url);
                }
            }
        }
    }
}
