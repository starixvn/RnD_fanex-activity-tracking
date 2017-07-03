namespace Fanex.ActivityTracking.Services
{
    using EasyNetQ;
    using Helpers;
    using Models;

    public interface IActivityLogger
    {
        void Write(Activity activity);
    }

    public class ActivityLogger : IActivityLogger
    {
        private readonly IBus bus;

        public ActivityLogger(IBus bus = null)
        {
            this.bus = bus ?? RabbitMqClient.Bus;
        }

        public void Write(Activity activity) => bus.Publish(activity);
    }
}