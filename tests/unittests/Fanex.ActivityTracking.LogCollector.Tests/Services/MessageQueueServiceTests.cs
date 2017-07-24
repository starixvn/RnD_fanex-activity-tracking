namespace Fanex.ActivityTracking.LogCollector.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ActivityTracking.Models;
    using EasyNetQ;
    using LogCollector.Services;
    using Logging;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class MessageQueueServiceTests
    {
        private IBus bus;
        private ActivityLogService activityLogService;
        private ILogger logger;
        private string subscriptionId;
        private MessageQueueServiceImpl messageQueueService;

        [SetUp]
        public void SetUp()
        {
            bus = Substitute.For<IBus>();
            activityLogService = Substitute.For<ActivityLogService>();
            logger = Substitute.For<ILogger>();
            subscriptionId = "subscriptionId";
            messageQueueService = new MessageQueueServiceImpl(
                activityLogService,
                bus,
                logger,
                subscriptionId);
        }

        [Test]
        public void SubcribeActivityLogs_Always_ExecuteSubscribe()
        {
            // Act
            messageQueueService.SubcribeActivityLogs();

            // Assert
            bus.Received().Subscribe(subscriptionId, (Action<Activity>)messageQueueService.InsertActivity);
        }

        [Test]
        public void InsertActivity_Always_ExecuteInsertActivity()
        {
            // Arrange 
            var activity = new Activity();

            // Act
            messageQueueService.InsertActivity(activity);

            // Assert
            activityLogService.Received().InsertActivity(activity);
        }

        [Test]
        public void InsertActivity_ThrowException_ExecuteLogError()
        {
            // Arrange 
            var activity = new Activity();
            var testError = new InvalidOperationException("Test Exception");
            activityLogService
                .When(logService =>
                {
                    logService.InsertActivity(Arg.Any<Activity>());
                })
                .Do((callInfo) =>
                {
                    throw testError;
                });

            // Act
            messageQueueService.InsertActivity(activity);

            // Assert
            logger.Received().Error("Test Exception", testError);
        }

        [Test]
        public void StopAllSubscriptions_Always_ExecuteDispose()
        {
            // Act
            messageQueueService.StopAllSubscriptions();

            // Assert
            bus.Received().Dispose();
        }
    }
}
