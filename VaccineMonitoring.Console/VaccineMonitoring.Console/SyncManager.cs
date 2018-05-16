using System;
using System.Net;
using System.Text;
using System.Timers;
using log4net;

namespace VaccineMonitoring.Console
{
    public class SyncManager
    {
        private readonly Timer timer = new Timer();

        private readonly int normalUpdateInterval = 600 * 1000; // 10 min
        //private readonly int normalUpdateInterval = 60 * 1000; // 1 min

        private readonly int retryUpdateInterval = 300 * 1000; // 5 min 

        private readonly MonitoringManager monitoringManager = new MonitoringManager();

        private bool IsRetryMode { get; set; }

        public SyncManager()
        {
            timer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            timer.Interval = normalUpdateInterval;
            timer.AutoReset = false;
        }

        public void Start()
        {
            timer.Start();
            Sync();
            LogManager.GetLogger("MonitoringLogger").Info("SyncManager Start");
        }

        public void Stop()
        {
            timer.Stop();
            LogManager.GetLogger("MonitoringLogger").Info("SyncManager Stop");
        }

        private void OnTimerEvent(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            try
            {
                Sync();
            }
            finally
            {
                timer.Interval = IsRetryMode
                    ? retryUpdateInterval
                    : normalUpdateInterval;

                timer.Start();
            }
        }

        private void Sync()
        {
            LogManager.GetLogger("MonitoringLogger").Info("Updating");
            try
            {
                monitoringManager.Run();

                IsRetryMode = false;
            }
            catch (WebException exception)
            {
                IsRetryMode = true;

                if (exception.Response != null)
                {
                    using (var responseStream = exception.Response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            byte[] bytes = new byte[responseStream.Length];
                            responseStream.Read(bytes, 0, bytes.Length);
                            LogManager.GetLogger("MonitoringLogger").Error(Encoding.UTF8.GetString(bytes));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                IsRetryMode = true;
                LogManager.GetLogger("MonitoringLogger").Error(exception.Message, exception);
            }

            LogManager.GetLogger("MonitoringLogger").Info("Updated");
        }
    }
}
