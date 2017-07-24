namespace Fanex.ActivityTracking.LogCollector
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.ServiceProcess;
    using ActivityTracking.Configurations;
    using Fanex.ActivityTracking.LogCollector.Services;
    using Logging;

    partial class TrackingActivityService : ServiceBase
    {
        private readonly MessageQueueService messageQueueService;

        public TrackingActivityService()
        {
            InitializeComponent();
            try
            {
                AppSettings.Init(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), AppSettings.ActivityTrackingConfigFilePath));
                messageQueueService = new MessageQueueServiceImpl();
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                messageQueueService.SubcribeActivityLogs();
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);
            }
        }

        protected override void OnStop()
        {
            messageQueueService.StopAllSubscriptions();
        }
    }
}