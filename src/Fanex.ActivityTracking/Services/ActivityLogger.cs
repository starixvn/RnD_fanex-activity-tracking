namespace Fanex.ActivityTracking.Services
{
    using EasyNetQ;
    using Helpers;
    using Models;

    public interface IActivityLogger
    {
        bool IsConnected();

        void Write(Activity activity);
    }

    public class ActivityLogger : IActivityLogger
    {
        private readonly IBus bus;

        public ActivityLogger(IBus bus = null)
        {
            this.bus = bus ?? RabbitMqClient.Bus;
        }

        public bool IsConnected()
        {
            return bus.IsConnected;
        }

        public void Write(Activity activity)
        {
            if (IsConnected())
            {
                bus.Publish(activity);
            }
        }
    }
}
