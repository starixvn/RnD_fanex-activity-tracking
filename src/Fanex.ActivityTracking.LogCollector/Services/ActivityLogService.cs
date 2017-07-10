namespace Fanex.ActivityTracking.LogCollector.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using ActivityTracking.Configurations;
    using ActivityTracking.Models;
    using ActivityTracking.Services;
    using Elasticsearch.Net;
    using Helpers;
    using Logging;
    using Nest;

    public interface ActivityLogService
    {
        Models.Result InsertActivity(Activity activity);

        IEnumerable<Activity> GetActivities(string system);
    }

    public class ActivityLogServiceImpl : ActivityLogService
    {
        private readonly IElasticClient elasticClient;
        private readonly TrackingService trackingService;
        private readonly ILogger logger;
        private static readonly char[] SplitCharForQueryString = new[] { '&' };
        private const string regexQueryString = "(?:{0}=)(?<paramValue>[^&]+)";
        private const string regexBodyJson = "(\"{0}\":)(?<paramValue>[^,]+)";
        private const string paramValue = "paramValue";

        public ActivityLogServiceImpl(
            IElasticClient elasticClient = null,
            TrackingService trackingService = null,
            ILogger logger = null)
        {
            this.elasticClient = elasticClient ?? ElasticsearchClient.Instance;
            this.trackingService = trackingService ?? new TrackingServiceImpl(AppSettings.ActivityTrackingConfig.EnabledTrackingAllRequests);
            this.logger = logger ?? Logger.Log;
        }

        public IEnumerable<Activity> GetActivities(string system)
        {
            return elasticClient.Search<Activity>(
                                s =>
                                s.Type(system)
                                .From(0)
                                .Size(5)
                                .Query(q => q.MatchAll()))
                                .Hits
                                .Select(hit => hit.Source);
        }

        public Models.Result InsertActivity(Activity activity)
        {
            var trackingItem = trackingService.GetTrackingItem(activity.UrlAccessed, AppSettings.ActivityTrackingConfig.TrackingItems);

            if (trackingItem == null
                || trackingItem.Level == TrackingLevel.NoTracking)
            {
                return Models.Result.SuccessResult;
            }

            if (string.IsNullOrEmpty(activity.Action))
            {
                activity.Action = trackingItem.Action;
            }

            activity.RequestInfo.Others = ExtractDataFromRequest(activity, trackingItem);

            var response = elasticClient.Index(
                    activity,
                    i => i
                    .Type(activity.System)
                    .Id(Guid.NewGuid().ToString())
                    .Refresh(Refresh.True));

            return response.IsValid ? Models.Result.SuccessResult : Models.Result.CreateErrorResult(response.ToString());
        }

        private string ExtractDataFromRequest(Activity activity, TrackingItem trackingItem)
        {
            var queryStringAndFormData = ExtractDataFromQueryString(
                (activity.RequestInfo.QueryString ?? string.Empty) + (activity.RequestInfo.Form ?? string.Empty),
                trackingItem.ParamInfoList);

            var bodyData = ExtractDataFromBodyRequest(activity.RequestInfo.BodyData, trackingItem.ParamInfoList);

            if (string.IsNullOrWhiteSpace(queryStringAndFormData)
                && string.IsNullOrWhiteSpace(bodyData))
            {
                return string.Empty;
            }

            return $"{queryStringAndFormData} \r\n {bodyData}";
        }

        private string ExtractDataFromQueryString(string queryString, IEnumerable<ParamInfo> paramInfoList)
        {
            if (string.IsNullOrEmpty(queryString)
                || paramInfoList == null
                || !paramInfoList.Any())
            {
                return string.Empty;
            }

            return GetDataFromRegex(queryString, regexQueryString, paramInfoList);
        }

        private string ExtractDataFromBodyRequest(string body, IEnumerable<ParamInfo> paramInfoList)
        {
            if (string.IsNullOrWhiteSpace(body)
                || !body.Contains("{"))
            {
                return string.Empty;
            }

            body = body.Replace("\\", "").Replace("\\", "");

            return GetDataFromRegex(body, regexBodyJson, paramInfoList);
        }

        private string GetDataFromRegex(string text, string pattern, IEnumerable<ParamInfo> paramInfoList)
        {
            var extractedDataBuilder = new StringBuilder();

            foreach (var paramInfo in paramInfoList)
            {
                var regexMatch = Regex.Match(text, string.Format(pattern, paramInfo.ActualName), RegexOptions.IgnoreCase);

                if (regexMatch.Success)
                {
                    extractedDataBuilder.Append($"{paramInfo.FriendlyName ?? paramInfo.ActualName} = {regexMatch.Groups[paramValue]} \r\n");
                }
            }

            return extractedDataBuilder.ToString();
        }
    }
}