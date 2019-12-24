namespace Fanex.ActivityTracking.Configurations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Common;
    using Models;

    [DataContract]
    public class ActivityTrackingConfig
    {
        private const int MaxRabbitMQConnectFailedCount = 3;

        [DataMember(Name = "rabbitMQConnectionString")]
        public string RabbitMQConnectionString { get; set; }

        [DataMember(Name = "isConnectionStringEncrypted")]
        public bool IsConnectionStringEncrypted { get; set; }

        [DataMember(Name = "trackingItems")]
        public IEnumerable<TrackingItem> TrackingItems { get; set; }

        [DataMember(Name = "enabledTrackAllRequests", IsRequired = false)]
        public bool EnabledTrackingAllRequests { get; set; }

        [DataMember(Name = "system")]
        public string System { get; set; }

        [DataMember(Name = "environment")]
        public string Environment { get; set; }

        [DataMember(Name = "trackProcessDuration")]
        public bool TrackProcessDuration { get; set; }

        [DataMember(Name = "site", IsRequired = false)]
        public string Site { get; set; }

        [DataMember(Name = "logCategory", IsRequired = false)]
        public string LogCategory { get; set; }

        [DataMember(Name = "ignoredUrls", IsRequired = false)]
        public IEnumerable<string> IgnoredUrls { get; set; }

        [DataMember(Name = "stopRabbitMQConnectionAfterFailedCount", IsRequired = false)]
        public int StopRabbitMQConnectionAfterFailedCount { get; set; }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            TrackingItems = Enumerable.Empty<TrackingItem>();
            IgnoredUrls = Enumerable.Empty<string>();
            StopRabbitMQConnectionAfterFailedCount = MaxRabbitMQConnectFailedCount;
            IsConnectionStringEncrypted = true;
            EnabledTrackingAllRequests = true;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (IsConnectionStringEncrypted
                && !string.IsNullOrWhiteSpace(RabbitMQConnectionString))
            {
                RabbitMQConnectionString = Cryptography.DeCryptConnectionString(RabbitMQConnectionString);
                RabbitMQConnectionString = RabbitMQConnectionString.Substring(0, RabbitMQConnectionString.Length - 1);
            }
        }
    }
}
