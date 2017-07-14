namespace Fanex.ActivityTracking.Models
{
    public class RequestInfo
    {
        public string HttpMethod { get; set; }

        public string Form { get; set; }

        public string QueryString { get; set; }

        public string BodyData { get; set; }

        public string HttpData { get; set; }

        public string Others { get; set; }
    }
}