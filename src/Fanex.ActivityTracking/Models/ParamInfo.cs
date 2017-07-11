namespace Fanex.ActivityTracking.Models
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class ParamInfo
    {
        [DataMember(Name = "actualNames", IsRequired = true)]
        public IEnumerable<string> ActualNames { get; set; }

        [DataMember(Name = "friendlyName", IsRequired = false)]
        public string FriendlyName { get; set; }
    }
}