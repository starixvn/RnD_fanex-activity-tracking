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
                    ActivityTrackingConfig = ToLowerAllUrls((ActivityTrackingConfig)serializer.ReadObject(stream));
                }
            }
            else
            {
                throw new FileNotFoundException($"Cannot find activity tracking json file: {configFilePath}");
            }
        }

        internal static ActivityTrackingConfig ToLowerAllUrls(ActivityTrackingConfig activityTrackingConfig)
        {
            if (activityTrackingConfig.TrackingItems != null
               && activityTrackingConfig.TrackingItems.Any())
            {
                foreach (var trackingItem in activityTrackingConfig.TrackingItems)
                {
                    if (trackingItem.Urls != null && trackingItem.Urls.Any())
                    {
                        trackingItem.Urls = trackingItem.Urls.Select(url => url.ToLowerInvariant());
                    }
                }
            }

            if (activityTrackingConfig.IgnoredUrls != null
                && activityTrackingConfig.IgnoredUrls.Any())
            {
                activityTrackingConfig.IgnoredUrls = activityTrackingConfig.IgnoredUrls.Select(url => url.ToLowerInvariant());
            }

            return activityTrackingConfig;
        }
    }
}