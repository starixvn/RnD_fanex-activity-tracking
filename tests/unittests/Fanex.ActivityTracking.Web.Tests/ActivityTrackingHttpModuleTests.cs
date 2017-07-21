namespace Fanex.ActivityTracking.Web.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.SessionState;
    using Configurations;
    using Logging;
    using Models;
    using NSubstitute;
    using NUnit.Framework;
    using Services;

    [TestFixture]
    public class ActivityTrackingHttpModuleTests
    {
        private ActivityTrackingHttpModule activityTrackingComponent;
        private IActivityLogger activityLogger;
        private IUserSession userSession;
        private IRequestAnalyzer requestAnalyzer;
        private IPService ipService;
        private TrackingService trackingService;
        private ILogger logger;
        private readonly Action initComponent = () => { };
        private ActivityTrackingConfig activityTrackingConfig;

        [SetUp]
        public void SetUp()
        {
            activityLogger = Substitute.For<IActivityLogger>();
            userSession = Substitute.For<IUserSession>();
            requestAnalyzer = Substitute.For<IRequestAnalyzer>();
            ipService = Substitute.For<IPService>();
            trackingService = Substitute.For<TrackingService>();
            logger = Substitute.For<ILogger>();
            activityTrackingConfig = new ActivityTrackingConfig
            {
                TrackingItems = new List<TrackingItem>
                {
                    new TrackingItem
                    {
                        Urls = new List<string>
                        {
                            "/path/test/1",
                            "/path/test/2"
                        },
                        Action = "Demo"
                    },
                    new TrackingItem
                    {
                        Urls = new List<string>
                        {
                            "/path/test/3",
                            "/path/test/4"
                        }
                    }
                },
                IgnoredUrls = new List<string>
                {
                    "/path/ignore/1",
                    "/path/ignore/2",
                    "/path/ignore/3"
                },
                System = "System",
                Environment = "Environment",
                Site = "Site"
            };

            activityTrackingComponent = new ActivityTrackingHttpModule(
                userSession,
                requestAnalyzer,
                trackingService,
                activityLogger,
                ipService,
                logger,
                activityTrackingConfig,
                initComponent);
        }

        [Test]
        public void SessionIsNull_SessionIsNull_ReturnTrue()
        {
            // Act & Assert
            Assert.IsTrue(activityTrackingComponent.SessionIsNull);
        }

        [Test]
        public void SessionIsNull_AdminAndCustIdIsZero_ReturnTrue()
        {
            // Arrange
            FakeHttpContext($"/path/query");

            // Act & Assert
            Assert.IsTrue(activityTrackingComponent.SessionIsNull);
        }

        [Test]
        public void SessionIsNull_AdminIdIsNotZero_ReturnFalse()
        {
            // Arrange
            FakeHttpContext($"/path/query");
            userSession.AdminId.Returns(1);

            // Act & Assert
            Assert.IsFalse(activityTrackingComponent.SessionIsNull);
        }

        [Test]
        public void SessionIsNull_CustIdIsNotZero_ReturnFalse()
        {
            // Arrange
            FakeHttpContext($"/path/query");
            userSession.CustId.Returns(1);

            // Act & Assert
            Assert.IsFalse(activityTrackingComponent.SessionIsNull);
        }

        [Test]
        public void AcquireRequestState_AdminIdOrCustId_Return()
        {
            // Arrange
            FakeHttpContext($"/path/query");

            // Arrange & Act
            activityTrackingComponent.AcquireRequestState(null, null);

            // Assert
            trackingService.DidNotReceiveWithAnyArgs().GetTrackingItem(null, null, null);
        }

        [TestCase("js")]
        [TestCase("jpg")]
        [TestCase("png")]
        [TestCase("gif")]
        [TestCase("css")]
        [TestCase("js")]
        public void AcquireRequestState_FileIsIngored_Return(string staticResourceSuffix)
        {
            // Arrange
            userSession.CustId.Returns(1);
            FakeHttpContext($"/path/query");

            activityTrackingComponent.HttpRequest = new HttpRequest("test", $"http://path/query.{staticResourceSuffix}", string.Empty);

            // Arrange & Act
            activityTrackingComponent.AcquireRequestState(null, null);

            // Assert
            trackingService.DidNotReceiveWithAnyArgs().GetTrackingItem(null, null, null);
        }

        [Test]
        public void AcquireRequestState_Always_ExecuteGetTrackingItem()
        {
            // Arrange
            userSession.CustId.Returns(1);
            FakeHttpContext("/path/query");
            var httpRequest = new HttpRequest("test", "http://test.com", string.Empty);
            activityTrackingComponent.HttpRequest = httpRequest;

            // Act
            activityTrackingComponent.AcquireRequestState(null, null);

            // Assert
            trackingService.Received().GetTrackingItem(
                httpRequest.Url.AbsolutePath,
                activityTrackingConfig.TrackingItems,
                activityTrackingConfig.IgnoredUrls);
        }

        [Test]
        public void AcquireRequestState_ActionNeedBeTracked_ExecuteWriteActivity()
        {
            // Arrange
            userSession.CustId.Returns(1);
            userSession.CustName.Returns("CustName");
            userSession.AdminId.Returns(2);
            userSession.AdminName.Returns("AdminName");
            userSession.SessionID.Returns("SessionID");

            FakeHttpContext("/path/query");

            activityTrackingComponent.HttpRequest = new HttpRequest("test", "http://test.com", string.Empty);

            trackingService
                .GetTrackingItem(Arg.Any<string>(), Arg.Any<IEnumerable<TrackingItem>>(), Arg.Any<IEnumerable<string>>())
                .Returns(new TrackingItem { Action = "Test", Level = TrackingLevel.Basic });

            requestAnalyzer.Analyze(Arg.Any<HttpRequest>()).Returns(new RequestInfo { QueryString = "QueryString" });
            ipService.GetClientIPAddress(Arg.Any<HttpRequest>()).Returns("IPAddress");

            // Act
            activityTrackingComponent.AcquireRequestState(null, null);

            // Assert
            activityLogger.Received().Write(
                Arg.Is<Activity>(activity
                => activity.Actor.AdminId == 2
                    && activity.Actor.AdminName == "AdminName"
                    && activity.Actor.CustId == 1
                    && activity.Actor.CustName == "CustName"
                    && activity.Action == "Test"
                    && activity.SessionID == "SessionID"
                    && activity.RequestInfo.QueryString == "QueryString"
                    && activity.IPAddress == "IPAddress"
                    && activity.UrlAccessed == activityTrackingComponent.HttpRequest.Url.ToString()
                    && activity.System == activityTrackingConfig.System
                    && activity.Site == activityTrackingConfig.Site
                    && activity.Environment == activityTrackingConfig.Environment));
        }

        [Test]
        public void AcquireRequestState_ThrowException_ExecuteLogError()
        {
            // Arrange
            userSession.CustId.Returns(1);
            FakeHttpContext("/path/query");

            var testException = new Exception("Test Exception");
            trackingService.When(trackingService => trackingService.GetTrackingItem(Arg.Any<string>(), Arg.Any<IEnumerable<TrackingItem>>(), Arg.Any<IEnumerable<string>>()))
                .Do((a) => { throw testException; });

            AppSettings.LogCategory = "LogCategory";

            // Act
            activityTrackingComponent.AcquireRequestState(null, null);

            // Assert
            logger.Received().Error("Test Exception", "LogCategory", testException);
        }

        private void FakeHttpContext(string queryString)
        {
            SimpleWorkerRequest request = new SimpleWorkerRequest(string.Empty, string.Empty, string.Empty, queryString, new StringWriter());

            HttpContext.Current = new HttpContext(request);

            var sessionContainer = new HttpSessionStateContainer(
                                                "id",
                                               new SessionStateItemCollection(),
                                               new HttpStaticObjectsCollection(),
                                               10,
                                               true,
                                               HttpCookieMode.AutoDetect,
                                               SessionStateMode.InProc,
                                               false);
            HttpContext.Current.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
                                    BindingFlags.NonPublic | BindingFlags.Instance,
                                    null,
                                    CallingConventions.Standard,
                                    new[] { typeof(HttpSessionStateContainer) },
                                    null)
                .Invoke(new object[] { sessionContainer });
        }
    }
}