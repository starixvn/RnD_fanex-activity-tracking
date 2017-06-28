namespace Fanex.ActivityTracking.LogCollector.Helpers
{
    using System;
    using EasyNetQ;
    using Logging;

    public class EasyNetQLogger : IEasyNetQLogger
    {
        public void DebugWrite(string format, params object[] args)
        {
            // Do Nothing
        }

        public void ErrorWrite(Exception exception)
        {
            Logger.Log.Error(exception.Message, exception);
        }

        public void ErrorWrite(string format, params object[] args)
        {
            Logger.Log.Error(string.Format(format, args));
        }

        public void InfoWrite(string format, params object[] args)
        {
            // Do Nothing
        }
    }
}
