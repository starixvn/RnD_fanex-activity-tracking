namespace Fanex.ActivityTracking.LogCollector.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ActivityTracking.Configurations;
    using ActivityTracking.Models;
    using ActivityTracking.Services;
    using Elasticsearch.Net;
    using Helpers;
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

        public ActivityLogServiceImpl(
            IElasticClient elasticClient = null,
            TrackingService trackingService = null)
        {
            this.elasticClient = elasticClient ?? ElasticsearchClient.Instance;
            this.trackingService = trackingService ?? new TrackingServiceImpl(AppSettings.ActivityTrackingConfig.EnabledTrackingAllRequests);
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
            if(trackingService.GetTrackingItem(activity.UrlAccessed, AppSettings.ActivityTrackingConfig.TrackingItems) == null)
            {
                return Models.Result.SuccessResult;
            }

            var response = elasticClient.Index(
                    activity,
                    i => i
                    .Type(activity.System)
                    .Id(Guid.NewGuid().ToString())
                    .Refresh(Refresh.True));

            return response.IsValid ? Models.Result.SuccessResult : Models.Result.CreateErrorResult(response.ToString());
        }
    }
}