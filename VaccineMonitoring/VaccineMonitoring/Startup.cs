using System;
using System.Collections.Generic;
using System.Linq;
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

            new SyncManager().Start();
        }
    }
}
