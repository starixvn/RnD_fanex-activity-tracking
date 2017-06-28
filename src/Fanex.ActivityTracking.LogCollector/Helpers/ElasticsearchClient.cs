namespace Fanex.ActivityTracking.LogCollector.Helpers
{
    using System;
    using System.Linq;
    using System.Net;
    using Configurations;
    using Elasticsearch.Net;
    using Nest;

    public static class ElasticsearchClient
    {
        private static IElasticClient elasticClient;

        private static readonly string username;

        private static readonly string password;

        static ElasticsearchClient()
        {
            var credential = AppSettings.ElasticsearchAuthInfo.Split(':');

            username = credential.ElementAt(0);
            password = credential.ElementAt(1);
        }

        public static IElasticClient Instance
        {
            get
            {
                if (elasticClient == null)
                {
                    var connectionPool = new StaticConnectionPool(AppSettings.UriList);
                    var connectionSettings = (new ConnectionSettings(connectionPool))
                        .DefaultIndex(AppSettings.ElasticsearchIndex);

                    if (AppSettings.IsSSL)
                    {
                        ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, errors) => true;
                        connectionSettings = connectionSettings
                            .SniffOnStartup()
                            .BasicAuthentication(username, password)
                            .PingTimeout(new TimeSpan(0, 1, 0))
                            .RequestTimeout(new TimeSpan(0, 1, 0));
                    }

                    elasticClient = new ElasticClient(connectionSettings);
                }

                return elasticClient;
            }
        }
    }
}