namespace Fanex.ActivityTracking.LogCollector.Services
{
    using System;
    using ActivityTracking.Helpers;
    using ActivityTracking.Models;
    using Configurations;
    using EasyNetQ;
    using Logging;

    public interface MessageQueueService
    {
        void SubcribeActivityLogs();

        void StopAllSubscriptions();
    }

    public class MessageQueueServiceImpl : MessageQueueService
    {
        private readonly IBus bus;
        private readonly ActivityLogService activityLogService;
        private readonly ILogger logger;
        private readonly string subscriptionId;

        public MessageQueueServiceImpl(
            ActivityLogService activityLogService = null,
            IBus bus = null,
            ILogger logger = null,
            string subscriptionId = null)
        {
            this.bus = bus ?? RabbitMqClient.Bus;
            this.activityLogService = activityLogService ?? new ActivityLogServiceImpl();
            this.logger = logger ?? Logger.Log;
            this.subscriptionId = subscriptionId ?? AppSettings.SubscriptionId;
        }

        public void SubcribeActivityLogs()
        {
            bus.Subscribe<Activity>(subscriptionId, InsertActivity);
        }

        internal void InsertActivity(Activity activity)
        {
            try
            {
                activityLogService.InsertActivity(activity);
            }
            catch (Exception ex)
            {
                LoggingWrapper.Error(logger, ex);
            }
        }

        public void StopAllSubscriptions()
        {
            bus.Dispose();
        }
    }
}
