namespace Fanex.ActivityTracking.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    public class ParamInfo
    {
        [DataMember(Name = "actualName", IsRequired = true)]
        public string ActualName { get; set; }

        [DataMember(Name = "friendlyName", IsRequired = false)]
        public string FriendlyName { get; set; }
    }
}