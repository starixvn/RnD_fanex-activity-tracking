namespace Fanex.ActivityTracking.LogCollector
{
    using System;
    using System.ServiceProcess;
    using Fanex.ActivityTracking.LogCollector.Services;

    partial class TrackingActivityService : ServiceBase
    {
        private readonly MessageQueueService messageQueueService;

        public TrackingActivityService()
        {
            InitializeComponent();
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