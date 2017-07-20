namespace Fanex.ActivityTracking.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ActivityTracking.Configurations;
    using ActivityTracking.Services;
    using Models;
    using NUnit.Framework;

    [TestFixture]
    public class TrackingServiceTests
    {
        private TrackingServiceImpl trackingService;
        private ActivityTrackingConfig activityTrackingConfig;

        [SetUp]
        public void SetUp()
        {
            activityTrackingConfig = new ActivityTrackingConfig();
            trackingService = new TrackingServiceImpl(activityTrackingConfig);
        }

        [Test]
        public void GetTrackingItem_UrlIsNullOrEmpty_ThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => trackingService.GetTrackingItem(
                null,
                Enumerable.Empty<TrackingItem>(),
                Enumerable.Empty<string>()));
        }

        [TestCase("/path/Test/1")]
        [TestCase("/path/test/1")]
        [TestCase("/Path/test/1")]
        public void GetTrackingItem_MatchedTrackingItem_ReturnTrackingItem(string url)
        {
            // Arrange
            var trackingItems = new List<TrackingItem>
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
            };

            // Act
            var actionItem = trackingService.GetTrackingItem(url, trackingItems, Enumerable.Empty<string>());

            // Assert
            Assert.AreSame(trackingItems.ElementAt(0), actionItem);
        }

        [TestCase("/path/Test/5")]
        [TestCase("/path/test/6")]
        public void GetTrackingItem_NotMatchedTrackingItem_ReturnNoTrackingItem(string url)
        {
            // Arrange
            var trackingItems = new List<TrackingItem>
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
            };

            // Act
            var actionItem = trackingService.GetTrackingItem(url, trackingItems, Enumerable.Empty<string>());

            // Assert
            Assert.AreSame(TrackingItem.NoTrackingItem, actionItem);
        }

        [TestCase("/path/ignore/1")]
        [TestCase("/path/Ignore/1")]
        [TestCase("/path/ignore/2")]
        public void GetTrackingItem_EnabledCheckeAllAndUrlIgnored_ReturnNoTrackingItem(string url)
        {
            // Arrange
            var ignoredUrls = new List<string>
            {
                "/path/ignore/1",
                "/path/ignore/2",
                "/path/ignore/3"
            };
            activityTrackingConfig.EnabledTrackingAllRequests = true;

            // Act
            var actionItem = trackingService.GetTrackingItem(url, Enumerable.Empty<TrackingItem>(), ignoredUrls);

            // Assert
            Assert.AreSame(TrackingItem.NoTrackingItem, actionItem);
        }

        [TestCase("/path/ignore/4")]
        [TestCase("/path/Ignore/5")]
        [TestCase("/path/ignore/6")]
        public void GetTrackingItem_EnabledCheckeAllAndUrlIsNotIgnored_ReturnBasicTrackingItem(string url)
        {
            // Arrange
            var ignoredUrls = new List<string>
            {
                "/path/ignore/1",
                "/path/ignore/2",
                "/path/ignore/3"
            };
            activityTrackingConfig.EnabledTrackingAllRequests = true;

            // Act
            var actionItem = trackingService.GetTrackingItem(url, Enumerable.Empty<TrackingItem>(), ignoredUrls);

            // Assert
            Assert.AreSame(TrackingItem.NoTrackingItem, actionItem);
        }
    }
}