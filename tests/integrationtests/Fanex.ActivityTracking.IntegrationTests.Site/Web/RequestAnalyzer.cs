namespace Fanex.ActivityTracking.IntegrationTests.Site.Web
{
    using Fanex.ActivityTracking.Web;

    public class RequestAnalyzer : BaseRequestAnalyzer
    {
        public RequestAnalyzer() : base(new JsonConverter())
        {
        }
    }
}