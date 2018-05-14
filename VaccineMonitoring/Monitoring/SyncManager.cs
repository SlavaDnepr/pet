using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Monitoring
{
    public class SyncManager
    {
        private readonly Timer timer = new Timer();

        //private readonly int normalUpdateInterval = 3600 * 1000; // 1 hour
        private readonly int normalUpdateInterval = 60 * 1000; // 1 hour

        private readonly int retryUpdateInterval = 300 * 1000; // 5 min 

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
            //Logger.Info(logType, Resources.Resources.SOFTWARE_REPOSITORY_STARTED_MESSAGE);
        }

        public void Stop()
        {
            timer.Stop();
            //Logger.Info(logType, Resources.Resources.SOFTWARE_REPOSITORY_STOPPED_MESSAGE);
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
            try
            {
                var monitoringManager = new MonitoringManager();
                
                monitoringManager.Run();
                
                IsRetryMode = false;
            }
            catch (WebException exception)
            {
                IsRetryMode = true;

                //if (exception.Response != null)
                //{
                //    using (Stream responseStream = exception.Response.GetResponseStream())
                //    {
                //        if (responseStream != null)
                //        {
                //            byte[] bytes = new byte[responseStream.Length];
                //            responseStream.Read(bytes, 0, bytes.Length);
                //            string message = string.Concat(Resources.Resources.SOFTWARE_REPOSITORY_ERROR_RESPONSE_MESSAGE, Environment.NewLine, RemoveTags(Encoding.UTF8.GetString(bytes)));
                //            Logger.Error(logType, message);
                //        }
                //    }
                //}
            }
            catch (Exception exception)
            {
                IsRetryMode = true;
                //Logger.Error(exception.Message, exception);
            }

            //Logger.Info(logType, Resources.Resources.SOFTWARE_REPOSITORY_UPDATED_MESSAGE);
        }
    }
}
