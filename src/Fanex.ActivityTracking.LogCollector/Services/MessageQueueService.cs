namespace Fanex.ActivityTracking.LogCollector.Services
{
    using System;
    using ActivityTracking.Models;
    using EasyNetQ;
    using Helpers;
    using Logging;

    public interface MessageQueueService
    {
        void SubcribeActivityLogs();

        void StopAllSubscriptions();
    }

    public class MessageQueueServiceImpl : MessageQueueService
    {
        private const string SubscriptionId = "Fanex.ActivityTracking.LogCollector.Services";
        private readonly IBus bus;
        private readonly ActivityLogService activityLogService;
        private readonly ILogger logger;

        public MessageQueueServiceImpl(
            ActivityLogService activityLogService = null,
            IBus bus = null,
            ILogger logger = null)
        {
            this.bus = bus ?? RabbitMqClient.Bus;
            this.activityLogService = activityLogService ?? new ActivityLogServiceImpl();
            this.logger = logger ?? Logger.Log;
        }

        public void SubcribeActivityLogs()
        {
            bus.Subscribe<Activity>(
            SubscriptionId,
            activity =>
            {
                try
                {
                    activityLogService.InsertActivity(activity);
                }
                catch(Exception ex)
                {
                    logger.Error(ex.Message, ex);
                }
            });
        }

        public void StopAllSubscriptions()
        {
            bus.Dispose();
        }
    }
}