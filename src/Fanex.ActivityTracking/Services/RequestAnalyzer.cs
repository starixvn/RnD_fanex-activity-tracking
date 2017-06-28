namespace Fanex.ActivityTracking.Services
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using Models;

    public interface IRequestAnalyzer
    {
        TrackingItem GetTrackingItem(string url, IEnumerable<TrackingItem> trackingItems);

        RequestInfo Analyze(HttpRequest request, TrackingLevel level);
    }

    public class BaseRequestAnalyzer : IRequestAnalyzer
    {
        private readonly bool enabledTrackAllRequests;
        private readonly IJsonConverter jsonConverter;

        public BaseRequestAnalyzer(
            IJsonConverter jsonConverter,
            bool enabledTrackAllRequests = false)
        {
            this.jsonConverter = jsonConverter;
            this.enabledTrackAllRequests = enabledTrackAllRequests;
        }

        public RequestInfo Analyze(HttpRequest request, TrackingLevel level)
        {
            var requestInfo = new RequestInfo();

            switch (level)
            {
                // No Request Data will be serialized
                case TrackingLevel.NoTracking:
                default:
                    return requestInfo;

                // Basic Request Serialization - just stores Data
                case TrackingLevel.Basic:
                    return new RequestInfo
                    {
                        Form = request.Form.ToString(),
                        QueryString = request.Url.Query
                    };

                // Middle Level - Customize to your Preferences
                case TrackingLevel.Middle:
                    return new RequestInfo
                    {
                        HttpMethod = request.HttpMethod,
                        Form = request.Form.ToString(),
                        QueryString = request.Url.Query,
                        BodyData = GetBodyData(request)
                    };

                // Highest Level - Serialize the entire Request object (As mentioned earlier, this will blow up)
                case TrackingLevel.Highest:
                    return new RequestInfo
                    {
                        HttpMethod = request.HttpMethod,
                        Form = request.Form.ToString(),
                        QueryString = request.Url.Query,
                        BodyData = GetBodyData(request),
                        Others = this.jsonConverter.SerializeObject(new { request.Cookies, request.Headers, request.Files, request.Params })
                    };
            }
        }

        private string GetBodyData(HttpRequest request)
        {
            Stream req = request.InputStream;
            req.Seek(0, SeekOrigin.Begin);
            var streamReader = new StreamReader(req);
            var bodyData = streamReader.ReadToEnd();
            req.Seek(0, SeekOrigin.Begin);
            if (!string.IsNullOrWhiteSpace(bodyData))
            {
                return HttpUtility.UrlDecode(bodyData);
            }

            return string.Empty;
        }

        public virtual TrackingItem GetTrackingItem(string url, IEnumerable<TrackingItem> trackingItems)
        {
            if (enabledTrackAllRequests)
            {
                return TrackingItem.MiddleLevelTrackingItem;
            }

            if (trackingItems.Any())
            {
                var urlToLower = url.ToLowerInvariant();
                foreach (var item in trackingItems)
                {
                    foreach (var itemUrl in item.Urls)
                    {
                        if (!string.IsNullOrWhiteSpace(itemUrl) 
                            && urlToLower.Contains(itemUrl))
                        {
                            return item;
                        }
                    }
                }
            }

            return null;
        }
    }
}