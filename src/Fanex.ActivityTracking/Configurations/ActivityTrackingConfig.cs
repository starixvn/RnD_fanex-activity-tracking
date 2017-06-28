namespace Fanex.ActivityTracking.Configurations
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Models;

    [DataContract]
    public class ActivityTrackingConfig
    {
        public ActivityTrackingConfig()
        {
            TrackingItems = Enumerable.Empty<TrackingItem>();
        }

        [DataMember(Name = "rabbitMQConnectionString")]
        public string RabbitMQConnectionString { get; set; }

        [DataMember(Name = "trackingItems")]
        public IEnumerable<TrackingItem> TrackingItems { get; set; }

        [DataMember(Name = "enabledTrackAllRequests", IsRequired = false)]
        public bool EnabledTrackingAllRequests { get; set; }

        [DataMember(Name = "system")]
        public string System { get; set; }

        [DataMember(Name = "environment")]
        public string Environment { get; set; }

        [DataMember(Name = "site", IsRequired = false)]
        public string Site { get; set; }
    }
}