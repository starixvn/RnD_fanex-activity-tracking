namespace Fanex.ActivityTracking.Tests.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ActivityTracking.Configurations;
    using Models;
    using NUnit.Framework;

    [TestFixture]
    public class AppSettingsTests
    {
        private ActivityTrackingConfig activityTrackingConfig;

        [SetUp]
        public void Setup()
        {
            activityTrackingConfig = new ActivityTrackingConfig();
        }

        [Test]
        public void ToLowerAllUrls_Always_ReturnTrackingItemUrlsTolowered()
        {
            // Arrange
            activityTrackingConfig.TrackingItems = new List<TrackingItem>
            {
                new TrackingItem
                {
                    Urls = new List<string>
                    {
                        "/path/TEST/1",
                        "/PATH/test/2"
                    },
                }
            };

            // Act
            var actualActivityTrackingConfig = AppSettings.ToLowerAllUrls(activityTrackingConfig);

            // Assert
            Assert.AreEqual("/path/test/1", actualActivityTrackingConfig.TrackingItems.ElementAt(0).Urls.ElementAt(0));
            Assert.AreEqual("/path/test/2", actualActivityTrackingConfig.TrackingItems.ElementAt(0).Urls.ElementAt(1));
        }

        [Test]
        public void ToLowerAllUrls_Always_ReturnIgnoredUrlsTolowered()
        {
            // Arrange
            activityTrackingConfig.IgnoredUrls = new List<string>()
            {
                "/path/TEST/1",
                "/PATH/test/2"
            };

            // Act
            var actualActivityTrackingConfig = AppSettings.ToLowerAllUrls(activityTrackingConfig);

            // Assert
            Assert.AreEqual("/path/test/1", actualActivityTrackingConfig.IgnoredUrls.ElementAt(0));
            Assert.AreEqual("/path/test/2", actualActivityTrackingConfig.IgnoredUrls.ElementAt(1));
        }
    }
}
