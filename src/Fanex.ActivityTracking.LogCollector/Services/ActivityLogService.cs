namespace Fanex.ActivityTracking.LogCollector.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using ActivityTracking.Configurations;
    using ActivityTracking.Models;
    using ActivityTracking.Services;
    using Elasticsearch.Net;
    using Helpers;
    using Nest;

    public interface ActivityLogService
    {
        Models.Result InsertActivity(Activity activity);
    }

    public class ActivityLogServiceImpl : ActivityLogService
    {
        private readonly IElasticClient elasticClient;
        private readonly TrackingService trackingService;
        private const string regexQueryString = "(?:{0}=)(?<paramValue>[^&]+)";
        private const string regexBodyJson = "(\"{0}\":)(?<paramValue>[^,]+)";
        private const string paramValue = "paramValue";
        private readonly ActivityTrackingConfig activityTrackingConfig;
        private readonly string elasticsearchIndexType;

        public ActivityLogServiceImpl(
            IElasticClient elasticClient = null,
            TrackingService trackingService = null,
            ActivityTrackingConfig activityTrackingConfig = null,
            string elasticsearchIndexType = null)
        {
            this.activityTrackingConfig = activityTrackingConfig ?? AppSettings.ActivityTrackingConfig;
            this.elasticClient = elasticClient ?? ElasticsearchClient.Instance;
            this.trackingService = trackingService ?? new TrackingServiceImpl(this.activityTrackingConfig);
            this.elasticsearchIndexType = elasticsearchIndexType ?? Configurations.AppSettings.ElasticsearchIndexType;
        }

        public Models.Result InsertActivity(Activity activity)
        {
            if (!string.IsNullOrWhiteSpace(activityTrackingConfig.System)
                && activity.System != activityTrackingConfig.System)
            {
                return Models.Result.SuccessResult;
            }

            var trackingItem = trackingService.GetTrackingItem(
                activity.UrlAccessed,
                activityTrackingConfig.TrackingItems,
                activityTrackingConfig.IgnoredUrls);

            if (trackingItem == null
                || trackingItem.Level == TrackingLevel.NoTracking)
            {
                return Models.Result.SuccessResult;
            }

            activity = AppendMoreInfo(activity, trackingItem);

            var response = elasticClient.Index(
                    activity,
                    i => i
                    .Type(elasticsearchIndexType)
                    .Refresh(Refresh.True));

            return response.IsValid 
                ? Models.Result.SuccessResult 
                : Models.Result.CreateErrorResult(response.ToString());
        }

        private Activity AppendMoreInfo(Activity activity, TrackingItem trackingItem)
        {
            if (string.IsNullOrEmpty(activity.Action))
            {
                activity.Action = trackingItem.Action;
            }

            if (string.IsNullOrEmpty(activity.Environment))
            {
                activity.Environment = activityTrackingConfig.Environment;
            }

            activity.RequestInfo = DecodeRequestInfo(activity.RequestInfo);

            if (activity.RequestInfo != null)
            {
                activity.RequestInfo.Others = ExtractDataFromRequest(activity, trackingItem);
            }

            return activity;
        }


        internal RequestInfo DecodeRequestInfo(RequestInfo requestInfo)
        {
            if(requestInfo == null)
            {
                return requestInfo;
            }

            requestInfo.QueryString = DecodeText(requestInfo.QueryString);
            requestInfo.Form = DecodeText(requestInfo.Form);
            requestInfo.BodyData = DecodeText(requestInfo.BodyData);

            return requestInfo;
        }

        internal string DecodeText(string text)
        {
            if(string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            var decodedText = Uri.UnescapeDataString(text);
            while(decodedText != text)
            {
                text = decodedText;
                decodedText = Uri.UnescapeDataString(text);
            }

            return decodedText;
        }

        internal string ExtractDataFromRequest(Activity activity, TrackingItem trackingItem)
        {
            var queryStringAndFormData = ExtractDataFromQueryString(
                (activity.RequestInfo.QueryString ?? string.Empty) + (activity.RequestInfo.Form ?? string.Empty),
                trackingItem.ParamInfoList);

            var bodyData = ExtractDataFromBodyRequest(activity.RequestInfo.BodyData, trackingItem.ParamInfoList);

            if (string.IsNullOrWhiteSpace(bodyData))
            {
                return queryStringAndFormData;
            }

            if (string.IsNullOrWhiteSpace(queryStringAndFormData))
            {
                return bodyData;
            }

            return $"{queryStringAndFormData}\r\n{bodyData}";
        }

        internal string ExtractDataFromQueryString(string queryString, IEnumerable<ParamInfo> paramInfoList)
        {
            if (string.IsNullOrEmpty(queryString)
                || paramInfoList == null
                || !paramInfoList.Any())
            {
                return string.Empty;
            }

            return GetDataFromRegex(queryString, regexQueryString, paramInfoList);
        }

        internal string ExtractDataFromBodyRequest(string body, IEnumerable<ParamInfo> paramInfoList)
        {
            if (string.IsNullOrWhiteSpace(body)
                || !body.Contains("{"))
            {
                return string.Empty;
            }

            body = body.Replace("\\", "").Replace("\\", "");

            return GetDataFromRegex(body, regexBodyJson, paramInfoList);
        }

        internal string GetDataFromRegex(string text, string pattern, IEnumerable<ParamInfo> paramInfoList)
        {
            var extractedDataDictionary = new Dictionary<string, string>();

            foreach (var paramInfo in paramInfoList)
            {
                foreach (var actualName in paramInfo.ActualNames)
                {
                    var regexMatch = Regex.Match(text, string.Format(pattern, actualName), RegexOptions.IgnoreCase);

                    if (regexMatch.Success)
                    {
                        var friendlyName = paramInfo.FriendlyName ?? actualName;
                        if (!extractedDataDictionary.ContainsKey(friendlyName))
                        {
                            extractedDataDictionary[friendlyName] = regexMatch.Groups[paramValue].ToString();
                        }
                    }
                }
            }

            return string.Join(",\r\n", extractedDataDictionary.Select(m => $"{m.Key} = {m.Value}"));
        }
    }
}