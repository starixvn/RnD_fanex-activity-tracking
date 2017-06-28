namespace Fanex.ActivityTracking.LogCollector.Helpers
{
    using Configurations;
    using EasyNetQ;

    public static class RabbitMqClient
    {
        private static IBus bus;

        public static IBus Bus
        {
            get
            {
                if (bus == null)
                {
                    bus = RabbitHutch.CreateBus(AppSettings.RabbitMQConnectionString,
                    serviceRegister =>
                    {
                        serviceRegister.Register<IEasyNetQLogger>(_ => new EasyNetQLogger());
                    });
                }

                return bus;
            }
        }
    }
}