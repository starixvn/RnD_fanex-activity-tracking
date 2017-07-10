namespace Fanex.ActivityTracking.Web
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.SessionState;
    using Configurations;
    using Logging;
    using Models;
    using Services;

    public class ActivityTrackingHttpModule : IHttpModule, IRequiresSessionState
    {
        private readonly IActivityLogger logger;
        private readonly IUserSession userSession;
        private readonly IRequestAnalyzer requestAnalyzer;
        private readonly IPService ipService;
        private readonly TrackingService trackingService;

        public ActivityTrackingHttpModule(
            Action initConfig,
            IUserSession userSession,
            IRequestAnalyzer requestAnalyzer,
            TrackingService trackingService = null,
            IActivityLogger logger = null,
            IPService ipService = null)
        {
            initConfig();
            this.userSession = userSession;
            this.requestAnalyzer = requestAnalyzer;
            this.trackingService = trackingService ?? new TrackingServiceImpl(AppSettings.ActivityTrackingConfig.EnabledTrackingAllRequests);
            this.logger = logger ?? new ActivityLogger();
            this.ipService = ipService ?? new IPServiceImpl();
        }

        private bool IgnoreStaticFiles
        {
            get
            {
                var pattern = "([^\\s]+(\\.(?i)(jpg|png|gif|bmp|css|js))$)";

                Regex urlRegex = new Regex(pattern);

                return urlRegex.IsMatch(HttpContext.Current.Request.Path, (int)RegexOptions.IgnoreCase);
            }
        }

        private bool SessionIsNull
        {
            get
            {
                return HttpContext.Current.Session == null
                    || (userSession.AdminId == 0 && userSession.CustId == 0);
            }
        }

        private static string GetUrl(HttpRequest request)
        {
            return string.IsNullOrWhiteSpace(request.Url.Query)
                                    ? request.Url.ToString()
                                    : request.Url.ToString().Replace(request.Url.Query, string.Empty);
        }

        public void Init(HttpApplication context)
        {
            context.AcquireRequestState += new EventHandler(AcquireRequestState);
        }

        private void AcquireRequestState(object sender, EventArgs e)
        {
            if (SessionIsNull || IgnoreStaticFiles)
            {
                return;
            }

            try
            {
                var request = HttpContext.Current.Request;
                var trackingItem = trackingService.GetTrackingItem(
                    request.Url.AbsolutePath,
                    AppSettings.ActivityTrackingConfig.TrackingItems);

                if (trackingItem != null
                    && trackingItem.Level != TrackingLevel.NoTracking)
                {
                    var activity = new Activity
                    {
                        Actor = new Actor
                        {
                            AdminId = userSession.AdminId,
                            AdminName = userSession.AdminName,
                            CustId = userSession.CustId,
                            CustName = userSession.CustName
                        },
                        Action = trackingItem.Action,
                        SessionID = userSession.SessionID,
                        TimeAccessed = DateTime.Now,
                        RequestInfo = requestAnalyzer.Analyze(request, trackingItem.Level),
                        IPAddress = ipService.GetClientIPAddress(request),
                        UrlAccessed = GetUrl(request),
                        System = AppSettings.ActivityTrackingConfig.System,
                        Environment = AppSettings.ActivityTrackingConfig.Environment,
                        Site = AppSettings.ActivityTrackingConfig.Site
                    };

                    logger.Write(activity);
                }
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, AppSettings.LogCategory, ex);
            }
        }

        public void Dispose()
        {
            // Nothing to release resource here.
        }
    }
}