namespace Fanex.ActivityTracking.Helpers
{
    using Configurations;
    using EasyNetQ;
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
    }
}
