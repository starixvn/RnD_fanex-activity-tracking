namespace Fanex.ActivityTracking.Tests.Services
{
    using ActivityTracking.Services;
    using EasyNetQ;
    using Models;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class ActivityLoggerTests
    {
        private ActivityLogger activityLogger;
        private IBus bus;

        [SetUp]
        public void Setup()
        {
            bus = Substitute.For<IBus>();
            activityLogger = new ActivityLogger(bus);
        }

        [Test]
        public void Write_BusConnected_ExecutePublishActivity()
        {
            // Arrange
            var activity = new Activity
            {
                Action = "demo",
                Site = "demo"
            };
            bus.IsConnected.Returns(true);

            // Act
            activityLogger.Write(activity);

            // Assert
            bus.Received().Publish(activity);
        }

        [Test]
        public void Write_BusDidnotConnect_NotExecutePublishActivity()
        {
            // Arrange
            var activity = new Activity
            {
                Action = "demo",
                Site = "demo"
            };
            bus.IsConnected.Returns(false);

            // Act
            activityLogger.Write(activity);

            // Assert
            bus.DidNotReceiveWithAnyArgs().Publish(activity);
        }
    }
}