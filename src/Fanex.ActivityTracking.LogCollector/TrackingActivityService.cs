namespace Fanex.ActivityTracking.LogCollector
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.ServiceProcess;
    using ActivityTracking.Configurations;
    using Fanex.ActivityTracking.Helpers;
    using Fanex.ActivityTracking.LogCollector.Services;

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
                LoggingWrapper.Error(ex);
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
                LoggingWrapper.Error(ex);
            }
        }

        protected override void OnStop()
        {
            messageQueueService.StopAllSubscriptions();
        }
    }
}
