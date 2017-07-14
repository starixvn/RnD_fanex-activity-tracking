namespace Fanex.ActivityTracking.Configurations
{
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Json;
    using Logging;

    public static class AppSettings
    {
        public static ActivityTrackingConfig ActivityTrackingConfig { get; internal set; }

        public static string LogCategory { get; internal set; }

        public static string ActivityTrackingConfigFilePath =>
            ConfigurationManager.AppSettings["ActivityTracking:ConfigFilePath"]
            ?? "~/App_Data/activitytracking.json";

        public static void Init(string configPath = null)
        {
            ReadAndDeserializeActivityTrackingConfig(configPath ?? ActivityTrackingConfigFilePath);
            LogCategory = ActivityTrackingConfig.LogCategory ?? "NAP-Activity-Tracking";
        }

        private static void ReadAndDeserializeActivityTrackingConfig(string configFilePath)
        {
            if (File.Exists(configFilePath))
            {
                using (Stream stream = File.OpenRead(configFilePath))
                {
                    var serializer = new DataContractJsonSerializer(typeof(ActivityTrackingConfig));
                    ActivityTrackingConfig = (ActivityTrackingConfig)serializer.ReadObject(stream);
                    ToLowerAllUrls();

                    LogService.Info($"Total tracking items: {ActivityTrackingConfig.TrackingItems.Count()}");
                }
            }
            else
            {
                throw new FileNotFoundException($"Cannot find activity tracking json file: {configFilePath}");
            }
        }

        private static void ToLowerAllUrls()
        {
            if (ActivityTrackingConfig.TrackingItems != null
               && ActivityTrackingConfig.TrackingItems.Any())
            {
                foreach (var trackingItem in ActivityTrackingConfig.TrackingItems)
                {
                    if (trackingItem.Urls != null && trackingItem.Urls.Any())
                    {
                        trackingItem.Urls = trackingItem.Urls.Select(url => url.ToLowerInvariant());
                    }
                }
            }

            if (ActivityTrackingConfig.IgnoredUrls != null
                && ActivityTrackingConfig.IgnoredUrls.Any())
            {
                ActivityTrackingConfig.IgnoredUrls = ActivityTrackingConfig.IgnoredUrls.Select(url => url.ToLowerInvariant());
            }
        }
    }
}