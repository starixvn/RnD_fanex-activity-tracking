namespace Fanex.ActivityTracking.Models
{
    using System;

    public class Activity
    {
        public Actor Actor { get; set; }

        // A new SessionId that will be used to link an entire
        // users "Session" of Audit Logs together to help
        // identifier patterns involving erratic behavior
        public string SessionID { get; set; }

        public string IPAddress { get; set; }

        public string UrlAccessed { get; set; }

        public string Action { get; set; }

        public DateTime TimeAccessed { get; set; }

        public DateTime? LastTimeAccessed { get; set; }

        public string Duration => LastTimeAccessed == null ? "0ms" : $"{(TimeAccessed - LastTimeAccessed).Value.Milliseconds}ms";

        // A new Data property that is going to store JSON
        // string objects that will later be able to be
        // deserialized into objects if necessary to view
        // details about a Request
        public RequestInfo RequestInfo { get; set; }

        public string System { get; set; }

        public string Environment { get; set; }

        public string Site { get; set; }
    }
}
