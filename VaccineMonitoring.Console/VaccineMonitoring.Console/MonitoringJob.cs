using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineMonitoring.Console
{
    public class MonitoringJob
    {
        public string Url { get; set; }

        public string Title { get; set; }

        public string LastResult { get; set; }

        public Warehouse Warehouse { get; set; }
    }
}
