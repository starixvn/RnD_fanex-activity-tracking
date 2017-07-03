namespace Fanex.ActivityTracking.Services
{
    using System;
    using Configurations;
    using EasyNetQ;
    using Logging;

    public class EasyNetQLogger : IEasyNetQLogger
    {
        public void DebugWrite(string format, params object[] args)
        {
            // Do Nothing
        }

        public void ErrorWrite(Exception exception) 
            => LogService.Error(exception.Message, AppSettings.LogCategory, exception);

        public void ErrorWrite(string format, params object[] args)
            => LogService.Error(string.Format(format, args), AppSettings.LogCategory);

        public void InfoWrite(string format, params object[] args)
        {
            // Do Nothing
        }
    }
}