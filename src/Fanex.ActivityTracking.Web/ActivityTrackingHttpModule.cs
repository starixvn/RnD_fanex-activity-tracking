namespace Fanex.ActivityTracking.Web
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.SessionState;
    using Configurations;
    using Fanex.ActivityTracking.Helpers;
    using Logging;
    using Models;
    using Services;

    public class ActivityTrackingHttpModule : IHttpModule, IRequiresSessionState
    {
        private static readonly Regex IgnoreStaticFilesPartern = new Regex("([^\\s]+(\\.(?i)(jpg|png|gif|bmp|css|js|map))$)");
        private readonly IActivityLogger activityLogger;
        private readonly IUserSession userSession;
        private readonly IRequestAnalyzer requestAnalyzer;
        private readonly IPService ipService;
        private readonly TrackingService trackingService;
        private readonly ILogger logger;
        private readonly IJsonConverter jsonConverter;
        private HttpRequest httpRequest;
        private HttpSessionState httpSession;
        private readonly ActivityTrackingConfig activityTrackingConfig;

        public ActivityTrackingHttpModule(
            IUserSession userSession,
            IRequestAnalyzer requestAnalyzer,
            IJsonConverter jsonConverter,
            TrackingService trackingService = null,
            IActivityLogger activityLogger = null,
            IPService ipService = null,
            ILogger logger = null,
            ActivityTrackingConfig activityTrackingConfig = null,
            Action initHttpModule = null)
        {
            try
            {
                initHttpModule = initHttpModule ?? InitHttpModule;
                initHttpModule();
                this.userSession = userSession;
                this.requestAnalyzer = requestAnalyzer;
                this.jsonConverter = jsonConverter;
                this.activityTrackingConfig = activityTrackingConfig ?? AppSettings.ActivityTrackingConfig;
                this.trackingService = trackingService ?? new TrackingServiceImpl(this.activityTrackingConfig);
                this.activityLogger = activityLogger ?? new ActivityLogger();
                this.ipService = ipService ?? new IPServiceImpl();
                this.logger = logger ?? Logger.Log;
            }
            catch
            {
            }
        }

        public static void InitHttpModule()
        {
            if (AppSettings.ActivityTrackingConfig == null
                && HttpContext.Current != null
                && HttpContext.Current.Server != null)
            {
                AppSettings.Init(HttpContext.Current.Server.MapPath(AppSettings.ActivityTrackingConfigFilePath));
            }
        }

        internal bool IgnoreStaticFiles => IgnoreStaticFilesPartern.IsMatch(HttpRequest.Url.ToString(), (int)RegexOptions.IgnoreCase);

        internal bool SessionIsNull => HttpSession == null
                    || (userSession.AdminId == 0 && userSession.CustId == 0);

        public void Init(HttpApplication context)
        {
            context.AcquireRequestState += new EventHandler(AcquireRequestState);

            if (AppSettings.ActivityTrackingConfig.TrackProcessDuration)
            {
                context.PostRequestHandlerExecute += new EventHandler(PostRequestHandlerExecute);
                context.Error += new EventHandler(Error);
            }
        }

        public HttpSessionState HttpSession
        {
            get
            {
                return httpSession ??
                    (HttpContext.Current != null
                    ? HttpContext.Current.Session
                    : null);
            }

            internal set
            {
                httpSession = value;
            }
        }

        public HttpRequest HttpRequest
        {
            get
            {
                return httpRequest ?? (HttpContext.Current != null ? HttpContext.Current.Request : null);
            }

            internal set
            {
                httpRequest = value;
            }
        }

        public bool IsActivityLoggerNotConnected => !activityLogger.IsConnected();

        public void AcquireRequestState(object sender, EventArgs e)
            => LogActivity(sender, e, SessionIsNull);

        public void PostRequestHandlerExecute(object sender, EventArgs e)
            => LogActivity(sender, e, false);

        public void Error(object sender, EventArgs e)
            => LogActivity(sender, e, false);

        public void LogActivity(object sender, EventArgs e, bool sessionIsNull)
        {
            try
            {
                if (sessionIsNull ||
                    IsActivityLoggerNotConnected ||
                    IgnoreStaticFiles)
                {
                    return;
                }

                var trackingItem = trackingService.GetTrackingItem(
                    HttpRequest.Url.AbsolutePath,
                    activityTrackingConfig.TrackingItems,
                    activityTrackingConfig.IgnoredUrls);

                if (trackingItem != null
                    && trackingItem.Level != TrackingLevel.NoTracking)
                {
                    LogActivity(trackingItem);
                }
            }
            catch (Exception ex)
            {
                LoggingWrapper.Error(logger, ex);
            }
        }

        private void LogActivity(TrackingItem trackingItem)
        {
            var key = userSession.AdminId + "_" + userSession.CustId + "_" + HttpRequest.Url.ToString();
            var activity = new Activity
            {
                Actor = new Actor
                {
                    AdminId = userSession.AdminId,
                    AdminName = userSession.AdminName,
                    CustId = userSession.CustId,
                    CustName = userSession.CustName,
                    UserSession = jsonConverter.SerializeObject(userSession)
                },
                Action = trackingItem.Action,
                TimeAccessed = DateTime.Now,
                LastTimeAccessed = AppSettings.ActivityTrackingConfig.TrackProcessDuration ? (DateTime?)HttpContext.Current.Session[key] : null,
                RequestInfo = requestAnalyzer.Analyze(HttpRequest),
                IPAddress = ipService.GetClientIPAddress(HttpRequest),
                UrlAccessed = HttpRequest.Url.ToString(),
                System = activityTrackingConfig.System,
                Environment = activityTrackingConfig.Environment,
                Site = activityTrackingConfig.Site
            };

            HttpContext.Current.Session[key] = DateTime.Now;
            activityLogger.Write(activity);
        }

        public void Dispose()
        {
            // Nothing to release resource here.
        }
    }
}
