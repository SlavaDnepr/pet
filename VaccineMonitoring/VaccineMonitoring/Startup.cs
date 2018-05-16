using log4net;
using Microsoft.Owin;
using Monitoring;
using Owin;

[assembly: OwinStartup(typeof(VaccineMonitoring.Startup))]

namespace VaccineMonitoring
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            LogManager.GetLogger("MonitoringLogger").Info("SyncManager starting...");
            new SyncManager().Start();
        }
    }
}