namespace Fanex.ActivityTracking.IntegrationTests.Site.Web
{
    using ActivityTracking.Web;

    public class IntegratingSiteActivityTrackingHttpModule : ActivityTrackingHttpModule
    {
        public IntegratingSiteActivityTrackingHttpModule()
            : base(new UserSession(), new RequestAnalyzer())
        {
        }
    }
}