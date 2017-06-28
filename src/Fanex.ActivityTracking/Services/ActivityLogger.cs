namespace Fanex.ActivityTracking.Services
{
    using Configurations;
    using EasyNetQ;
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
            this.bus = bus ?? AppSettings.Bus;
        }

        public void Write(Activity activity)
        {
            bus.Publish(activity);
        }
    }
}