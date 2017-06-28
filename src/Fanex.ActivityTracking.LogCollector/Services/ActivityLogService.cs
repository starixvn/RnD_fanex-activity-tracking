namespace Fanex.ActivityTracking.LogCollector.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ActivityTracking.Models;
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

        public ActivityLogServiceImpl(IElasticClient elasticClient = null)
        {
            this.elasticClient = elasticClient ?? ElasticsearchClient.Instance;
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
            var response = elasticClient.Index(
                    activity,
                    i => i
                    .Type(activity.System)
                    .Id(Guid.NewGuid().ToString())
                    .Refresh(Refresh.True)
                    );

            return response.IsValid ? Models.Result.SuccessResult : Models.Result.CreateErrorResult(response.ToString());
        }
    }
}