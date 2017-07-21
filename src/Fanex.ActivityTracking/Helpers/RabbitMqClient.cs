namespace Fanex.ActivityTracking.Helpers
{
    using Configurations;
    using EasyNetQ;
    using Logging;
    using Services;

    public static class RabbitMqClient
    {
        public static int NotConnectedFailedCount { get; set; }

        private static IBus bus;

        public static IBus Bus
        {
            get
            {
                if (bus == null)
                {
                    bus = RabbitHutch.CreateBus(
                        AppSettings.ActivityTrackingConfig.RabbitMQConnectionString,
                        serviceRegister =>
                        {
                            serviceRegister.Register<IEasyNetQLogger>(_ => new EasyNetQLogger());
                        });
                }

                return bus;
            }

            internal set
            {
                bus = value;
            }
        }

        public static void CloseBus()
        {
            if(bus != null)
            {
                bus.Dispose();
            }
        }

        public static void HandleFailedConnect()
        {
            if (NotConnectedFailedCount > AppSettings.ActivityTrackingConfig.StopRabbitMQConnectionAfterFailedCount)
            {
                LogService.Error($"Force close RabbitMQ because of connecting failed in {AppSettings.ActivityTrackingConfig.StopRabbitMQConnectionAfterFailedCount} times", AppSettings.LogCategory);
                CloseBus();
            }

            NotConnectedFailedCount++;
        }
    }
}