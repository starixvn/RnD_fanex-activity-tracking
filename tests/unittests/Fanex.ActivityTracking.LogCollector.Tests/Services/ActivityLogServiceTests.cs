namespace Fanex.ActivityTracking.LogCollector.Tests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using ActivityTracking.Configurations;
    using ActivityTracking.Models;
    using ActivityTracking.Services;
    using LogCollector.Services;
    using Nest;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class ActivityLogServiceTests
    {
        private ActivityLogServiceImpl activityLogService;
        private IElasticClient elasticClient;
        private TrackingService trackingService;
        private ActivityTrackingConfig activityTrackingConfig;

        [SetUp]
        public void SetUp()
        {
            elasticClient = Substitute.For<IElasticClient>();
            trackingService = Substitute.For<TrackingService>();
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

            activityLogService = new ActivityLogServiceImpl(elasticClient, trackingService, activityTrackingConfig);
        }

        [Test]
        public void InsertActivity_Always_ExecuteGetTrackingItem()
        {
            // Arrange
            var activity = new Activity
            {
                UrlAccessed = "UrlAccessed"
            };

            // Act
            activityLogService.InsertActivity(activity);

            // Assert
            trackingService.Received().GetTrackingItem(activity.UrlAccessed, activityTrackingConfig.TrackingItems, activityTrackingConfig.IgnoredUrls);
        }

        [Test]
        public void InsertActivity_TrackingItemIsNull_ReturnSuccessResult()
        {
            // Arrange
            var activity = new Activity();
            TrackingItem returnValue = null;
            trackingService
                .GetTrackingItem(Arg.Any<string>(), Arg.Any<IEnumerable<TrackingItem>>(), Arg.Any<IEnumerable<string>>())
                .Returns(returnValue);

            // Act
            var result = activityLogService.InsertActivity(activity);

            // Assert
            Assert.AreSame(Models.Result.SuccessResult, result);
        }

        [Test]
        public void InsertActivity_TrackingItemLevelIsNoTracking_ReturnSuccessResult()
        {
            // Arrange
            var activity = new Activity();
            trackingService
                .GetTrackingItem(Arg.Any<string>(), Arg.Any<IEnumerable<TrackingItem>>(), Arg.Any<IEnumerable<string>>())
                .Returns(TrackingItem.NoTrackingItem);

            // Act
            var result = activityLogService.InsertActivity(activity);

            // Assert
            Assert.AreSame(Models.Result.SuccessResult, result);
        }

        [Test]
        public void InsertActivity_ActivityHasNoActionName_AssignActionNameFromTrackingItem()
        {
            // Arrange
            var activity = new Activity();
            SetFakeTrackingItem(new TrackingItem
            {
                Action = "Action",
                Level = TrackingLevel.Basic
            });

            // Act
            activityLogService.InsertActivity(activity);

            // Assert
            Assert.AreSame("Action", activity.Action);
        }

        [TestCase("?query=1&demo=3", "query", "FriendlyName = 1")]
        [TestCase("?query=1&demo=3", "query,demo", "FriendlyName = 1")]
        [TestCase("?query=1&demo=3&demo3=3", "demo", "FriendlyName = 3")]
        public void InsertActivity_QueryStringContainsTrackingData_AssignTrackingData(string queryString, string paramNames, string expectedResult)
        {
            // Arrange
            var activity = new Activity
            {
                RequestInfo = new RequestInfo
                {
                    QueryString = queryString
                }
            };

            SetFakeTrackingItem(new TrackingItem
            {
                ParamInfoList = new List<ParamInfo> { new ParamInfo { ActualNames = paramNames.Split(',').AsEnumerable(), FriendlyName = "FriendlyName" } }
            });

            // Act
            activityLogService.InsertActivity(activity);

            // Assert
            Assert.AreEqual(expectedResult, activity.RequestInfo.Others);
        }

        [TestCase("query=1&demo=3", "query", "demo", "FriendlyName = 1,\r\nFriendlyName 2 = 3")]
        [TestCase("query=1&demo=3", "query,demo", "query,demo", "FriendlyName = 1,\r\nFriendlyName 2 = 1")]
        [TestCase("query=2&demo=3&demo3=3", "demo", "query", "FriendlyName = 3,\r\nFriendlyName 2 = 2")]
        public void InsertActivity_FormContainsTrackingData_AssignTrackingData(
            string bodyData, 
            string paramNames,
            string paramNames2,
            string expectedResult)
        {
            // Arrange
            var activity = new Activity
            {
                RequestInfo = new RequestInfo
                {
                    Form = bodyData
                }
            };

            SetFakeTrackingItem(new TrackingItem
            {
                ParamInfoList = new List<ParamInfo>
                {
                    new ParamInfo { ActualNames = paramNames.Split(',').AsEnumerable(), FriendlyName = "FriendlyName" },
                    new ParamInfo { ActualNames = paramNames2.Split(',').AsEnumerable(), FriendlyName = "FriendlyName 2" }
                }
            });

            // Act
            activityLogService.InsertActivity(activity);

            // Assert
            Assert.AreEqual(expectedResult, activity.RequestInfo.Others);
        }

        [TestCase("{\"query\":1,", "query", "demo", "FriendlyName = 1")]
        [TestCase("{\"query\":1,\"demo\":3,", "query,demo", "query,demo", "FriendlyName = 1,\r\nFriendlyName 2 = 1")]
        [TestCase("{\"query\":2,\"demo\":3,", "demo", "query", "FriendlyName = 3,\r\nFriendlyName 2 = 2")]
        public void InsertActivity_BodyDataContainsTrackingData_AssignTrackingData(
            string bodyData, 
            string paramNames, 
            string paramNames2,
            string expectedResult)
        {
            // Arrange
            var activity = new Activity
            {
                RequestInfo = new RequestInfo
                {
                    BodyData = bodyData
                }
            };

            SetFakeTrackingItem(new TrackingItem
            {
                ParamInfoList = new List<ParamInfo>
                {
                    new ParamInfo { ActualNames = paramNames.Split(',').AsEnumerable(), FriendlyName = "FriendlyName" },
                    new ParamInfo { ActualNames = paramNames2.Split(',').AsEnumerable(), FriendlyName = "FriendlyName 2" }
                }
            });

            // Act
            activityLogService.InsertActivity(activity);

            // Assert
            Assert.AreEqual(expectedResult, activity.RequestInfo.Others);
        }

        [TestCase("?query=1&demo=3", "{\"query\":3,", "query", "FriendlyName = 1\r\nFriendlyName = 3")]
        [TestCase("?query=1&demo=3", "{\"query\":4,\"demo\":3,", "query,demo", "FriendlyName = 1\r\nFriendlyName = 4")]
        [TestCase("?query=1&demo=10&demo3=3", "{\"query\":1,\"demo\":12,", "demo", "FriendlyName = 10\r\nFriendlyName = 12")]
        public void InsertActivity_QueryStringAndBodyDataContainTrackingData_AssignTrackingData(
            string queryString, 
            string bodyData, 
            string paramNames, 
            string expectedResult)
        {
            // Arrange
            var activity = new Activity
            {
                RequestInfo = new RequestInfo
                {
                    QueryString = queryString,
                    BodyData = bodyData
                }
            };

            SetFakeTrackingItem(new TrackingItem
            {
                ParamInfoList = new List<ParamInfo> { new ParamInfo { ActualNames = paramNames.Split(',').AsEnumerable(), FriendlyName = "FriendlyName" } }
            });

            // Act
            activityLogService.InsertActivity(activity);

            // Assert
            Assert.AreEqual(expectedResult, activity.RequestInfo.Others);
        }

        public void SetFakeTrackingItem(TrackingItem trackingItem)
        {
            trackingService
              .GetTrackingItem(Arg.Any<string>(), Arg.Any<IEnumerable<TrackingItem>>(), Arg.Any<IEnumerable<string>>())
              .Returns(trackingItem);
        }
    }
}