namespace Fanex.ActivityTracking.Helpers
{
    using System;
    using Fanex.ActivityTracking.Configurations;
    using Fanex.Logging;

    public static class LoggingWrapper
    {
        public static void Error(ILogger logger, Exception ex)
        {
            try
            {
                logger.Error(ex.Message, AppSettings.LogCategory, ex);
            }
            catch
            {
            }
        }

        public static void Error(Exception ex)
        {
            try
            {
                LogService.Error(ex.Message, ex);
            }
            catch
            {
            }
        }

        public static void Error(string message, string category)
        {
            try
            {
                LogService.Error(message, category);
            }
            catch
            {
            }
        }

        public static void Error(string message, string category, Exception exception)
        {
            try
            {
                LogService.Error(message, category, exception);
            }
            catch
            {
            }
        }
    }
}
