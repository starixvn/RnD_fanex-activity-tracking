namespace Fanex.ActivityTracking.LogCollector.Tests.Services
{
    using System;
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
        public void InsertActivity_ActivityHasDifferentSystem_ReturnDoNothingAndSuccessResult()
        {
            // Arrange
            var activity = new Activity
            {
                System = "System1"
            };

            // Act
            var result = activityLogService.InsertActivity(activity);

            // Assert
            Assert.AreSame(Models.Result.SuccessResult, result);
        }

        [Test]
        public void InsertActivity_Always_ExecuteGetTrackingItem()
        {
            // Arrange
            var activity = new Activity
            {
                UrlAccessed = "UrlAccessed",
                System = "System"
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
            var activity = new Activity
            {
                System = "System"
            };
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
            var activity = new Activity
            {
                System = "System"
            };
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
            var activity = new Activity
            {
                System = "System"
            };
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

        [Test]
        public void InsertActivity_ActivityHasNoEnvironment_AssignEnvironmentFromConfig()
        {
            // Arrange
            var activity = new Activity
            {
                System = "System"
            };
            SetFakeTrackingItem(new TrackingItem
            {
                Action = "Action",
                Level = TrackingLevel.Basic
            });
            activityTrackingConfig.Environment = "Environment1";

            // Act
            activityLogService.InsertActivity(activity);

            // Assert
            Assert.AreSame("Environment1", activity.Environment);
        }

        [TestCase("?query=1&demo=3", "query", "FriendlyName = 1")]
        [TestCase("?query=1&demo=3", "query,demo", "FriendlyName = 1")]
        [TestCase("?query=1&demo=3&demo3=3", "demo", "FriendlyName = 3")]
        public void InsertActivity_QueryStringContainsTrackingData_AssignTrackingData(string queryString, string paramNames, string expectedResult)
        {
            // Arrange
            var activity = new Activity
            {
                System = "System",
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
                System = "System",
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
                System = "System",
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
                System = "System",
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

        [Test]
        public void InsertActivity_Always_ExecuteElasticIndex()
        {
            // Arrange
            var activity = new Activity
            {
                System = "System"
            };
            SetFakeTrackingItem(new TrackingItem { Level = TrackingLevel.Basic });

            // Act
            activityLogService.InsertActivity(activity);

            // Assert
            elasticClient.Received().Index(activity, Arg.Any<Func<IndexDescriptor<Activity>, IIndexRequest<Activity>>>());
        }

        [Test]
        public void InsertActivity_FailedInsert_ReturnErrorResult()
        {
            // Arrange
            var activity = new Activity
            {
                System = "System"
            };
            SetFakeTrackingItem(new TrackingItem { Level = TrackingLevel.Basic });
            var indexResponse = Substitute.For<IIndexResponse>();
            elasticClient
                .Index(Arg.Any<Activity>(), Arg.Any<Func<IndexDescriptor<Activity>, IIndexRequest<Activity>>>())
                .Returns(indexResponse);

            indexResponse.IsValid.Returns(false);

            // Act
            var actualResult = activityLogService.InsertActivity(activity);

            // Assert
            Assert.IsFalse(actualResult.IsSuccessful);
        }

        [Test]
        public void InsertActivity_InsertSuccessful_ReturnSuccessResult()
        {
            // Arrange
            var activity = new Activity();
            SetFakeTrackingItem(new TrackingItem { Level = TrackingLevel.Basic });
            var indexResponse = Substitute.For<IIndexResponse>();
            elasticClient
                .Index(Arg.Any<Activity>(), Arg.Any<Func<IndexDescriptor<Activity>, IIndexRequest<Activity>>>())
                .Returns(indexResponse);

            indexResponse.IsValid.Returns(true);

            // Act
            var actualResult = activityLogService.InsertActivity(activity);

            // Assert
            Assert.AreEqual(Models.Result.SuccessResult, actualResult);
        }

        [TestCase("12b%2524viet", "12b$viet")]
        [TestCase("12b%24viet", "12b$viet")]
        [TestCase("12b%252524viet", "12b$viet")]
        [TestCase("http://ha.nexdev.net:7410/ex-main/dcs/viewcustomersetting?customerLevel=1&amp;UplineLevel=2&amp;customerId=1584729&amp;productId=28", 
            "http://ha.nexdev.net:7410/ex-main/dcs/viewcustomersetting?customerLevel=1&amp;UplineLevel=2&amp;customerId=1584729&amp;productId=28")]
        [TestCase("%7b%22AdminId%22%3a0%2c%22AdminName%22%3anull%2c%22CustId%22%3a1176086%2c%22CustName%22%3a%22AGENT01%22%2c",
            "{\"AdminId\":0,\"AdminName\":null,\"CustId\":1176086,\"CustName\":\"AGENT01\",")]
        [TestCase("%257b%2522AdminId%2522%253a0%252c%2522AdminName%2522%253anull%252c%2522CustId%2522%253a1176086%252c%2522CustName%2522%253a%2522AGENT01%2522%252c",
            "{\"AdminId\":0,\"AdminName\":null,\"CustId\":1176086,\"CustName\":\"AGENT01\",")]
        public void DecodeText_EncodedText_ReturnDecodedString(string encodedText, string expectedDecodedText)
        {
            // Act
            var actualDecodedText = activityLogService.DecodeText(encodedText);

            // Assert
            Assert.AreEqual(expectedDecodedText, actualDecodedText);
        }

        [Test]
        public void DecodeRequestInfo_RequestInfoIsEncoded_ReturnDecodedRequestInfo()
        {
            // Arrange
            var requestInfo = new RequestInfo
            {
                BodyData = "12b%2524viet",
                QueryString = "12b%24viet",
                Form = "12b%252524viet"
            };

            // Act
            var actualRequestInfo = activityLogService.DecodeRequestInfo(requestInfo);

            // Assert
            Assert.IsTrue(
                actualRequestInfo.BodyData == "12b$viet"
                && actualRequestInfo.Form == "12b$viet"
                && actualRequestInfo.QueryString == "12b$viet");
        }

        public void SetFakeTrackingItem(TrackingItem trackingItem)
        {
            trackingService
              .GetTrackingItem(Arg.Any<string>(), Arg.Any<IEnumerable<TrackingItem>>(), Arg.Any<IEnumerable<string>>())
              .Returns(trackingItem);
        }
    }
}