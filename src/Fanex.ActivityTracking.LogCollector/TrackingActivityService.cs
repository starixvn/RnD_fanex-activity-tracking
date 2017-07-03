namespace Fanex.ActivityTracking.LogCollector
{
    using System;
    using System.IO;
    using System.ServiceProcess;
    using ActivityTracking.Configurations;
    using Fanex.ActivityTracking.LogCollector.Services;

    partial class TrackingActivityService : ServiceBase
    {
        private readonly MessageQueueService messageQueueService;

        public TrackingActivityService()
        {
            InitializeComponent();
            AppSettings.Init(Path.Combine(Environment.CurrentDirectory, AppSettings.ActivityTrackingConfigFilePath));
            messageQueueService = new MessageQueueServiceImpl();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                messageQueueService.SubcribeActivityLogs();
            }
            catch (Exception ex)
            {
                Logging.LogService.Error(ex.Message, ex);
            }
        }

        protected override void OnStop()
        {
            messageQueueService.StopAllSubscriptions();
        }
    }
}