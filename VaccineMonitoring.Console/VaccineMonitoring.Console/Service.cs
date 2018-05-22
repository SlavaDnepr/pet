using System;
using System.ServiceProcess;
using log4net;

namespace VaccineMonitoring.Console
{
    public sealed class Service : ServiceBase
    {
        private Service()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            log4net.Config.XmlConfigurator.Configure();
            new SyncManager().Start();
        }

        static void Main(string[] args)
        {
            var service = new Service();

            if (Environment.UserInteractive)
            {
                service.OnStart(args);
                System.Console.WriteLine("Service started");
                System.Console.WriteLine("Press any key to stop program");
                System.Console.Read();
                service.OnStop();
            }
            else
                Run(service);
        }

        protected override void OnContinue()
        {
            OnStart(null);
        }

        protected override void OnPause()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }

        private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            LogManager.GetLogger("MonitoringLogger").Fatal(exception.Message, exception);
        }
    }
}