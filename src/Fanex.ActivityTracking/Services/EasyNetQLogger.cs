namespace Fanex.ActivityTracking.Services
{
    using System;
    using Configurations;
    using EasyNetQ;
    using Helpers;
    using Logging;

    public class EasyNetQLogger : IEasyNetQLogger
    {
        private const string ConnectFailedMessage = "Failed to connect to any Broke";
        public void DebugWrite(string format, params object[] args)
        {
            // We dont need to log this information.
        }

        public void ErrorWrite(Exception exception)
            => LogService.Error(exception.Message, AppSettings.LogCategory, exception);

        public void ErrorWrite(string format, params object[] args)
        {
            var formtedMessage = string.Format(format, args);

            if(formtedMessage.Contains(ConnectFailedMessage))
            {
                RabbitMqClient.HandleFailedConnect();
            }

            LogService.Error(formtedMessage, AppSettings.LogCategory);
        }

        public void InfoWrite(string format, params object[] args)
            => LogService.Info(string.Format(format, args), AppSettings.LogCategory);
    }
}