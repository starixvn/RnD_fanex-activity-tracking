namespace Fanex.ActivityTracking.Models
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class TrackingItem
    {
        public static readonly TrackingItem MiddleLevelTrackingItem = new TrackingItem { Level = TrackingLevel.Basic };
        public static readonly TrackingItem NoTrackingItem = new TrackingItem { Level = TrackingLevel.NoTracking };

        public TrackingItem()
        {
            Level = TrackingLevel.Basic;
        }

        [DataMember(Name = "function", IsRequired = false)]
        public string Function { get; set; }

        [DataMember(Name = "action", IsRequired = false)]
        public string Action { get; set; }

        [DataMember(Name = "urls", IsRequired = false)]
        public IEnumerable<string> Urls { get; set; }

        public TrackingLevel Level { get; set; }

        [DataMember(Name = "paramInfoList", IsRequired = false)]
        public IEnumerable<ParamInfo> ParamInfoList { get; set; }
    }
}