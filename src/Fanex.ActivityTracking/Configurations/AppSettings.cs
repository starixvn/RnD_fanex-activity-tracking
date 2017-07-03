namespace Fanex.ActivityTracking.Configurations
{
    using System.Linq;
    using System.Configuration;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Web;
    using EasyNetQ;
    using Services;

    public static class AppSettings
    {
        static AppSettings()
        {
            LoadSecurityCodeConfig();
            LogCategory = ActivityTrackingConfig.LogCategory ?? "NAP-Activity-Tracking";
        }

        public static ActivityTrackingConfig ActivityTrackingConfig { get; internal set; }

        public static string LogCategory { get; internal set; }

        private static IBus bus;

        public static string ActivityTrackingConfigFilePath
        {
            get
            {
                return ConfigurationManager.AppSettings["ActivityTracking:ConfigFilePath"] ?? "~/App_Data/activitytracking.json";
            }
        }

        public static IBus Bus
        {
            get
            {
                if (bus == null)
                {
                    bus = RabbitHutch.CreateBus(ActivityTrackingConfig.RabbitMQConnectionString,
                    serviceRegister =>
                    {
                        serviceRegister.Register<IEasyNetQLogger>(_ => new EasyNetQLogger());
                    });
                }

                return bus;
            }
        }

        private static void LoadSecurityCodeConfig()
        {
            if (HttpContext.Current != null
                && HttpContext.Current.Server != null)
            {
                var configFilePath = HttpContext.Current.Server.MapPath(ActivityTrackingConfigFilePath);
                if (File.Exists(configFilePath))
                {
                    using (Stream stream = File.OpenRead(configFilePath))
                    {
                        var serializer = new DataContractJsonSerializer(typeof(ActivityTrackingConfig));
                        ActivityTrackingConfig = (ActivityTrackingConfig)serializer.ReadObject(stream);
                        ToLowerAllUrls();
                    }
                }
                else
                {
                    throw new FileNotFoundException("Cannot find security code json file");
                }
            }
        }

        private static void ToLowerAllUrls()
        {
            foreach (var trackingItem in ActivityTrackingConfig.TrackingItems)
            {
                if(trackingItem.Urls != null && trackingItem.Urls.Any())
                {
                    trackingItem.Urls = trackingItem.Urls.Select(url => url.ToLowerInvariant());
                }
            }
        }
    }
}