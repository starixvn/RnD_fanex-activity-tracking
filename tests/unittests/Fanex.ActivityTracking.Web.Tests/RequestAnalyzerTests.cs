namespace Fanex.ActivityTracking.Web.Tests
{
    using System.Collections.Specialized;
    using System.Web;
    using NSubstitute;
    using NUnit.Framework;
    using Services;

    [TestFixture]
    public class RequestAnalyzerTests
    {
        private BaseRequestAnalyzer requestAnalyzer;
        private IJsonConverter jsonConverter;

        [SetUp]
        public void SetUp()
        {
            jsonConverter = Substitute.For<IJsonConverter>();
            requestAnalyzer = new BaseRequestAnalyzer(jsonConverter);
        }

        [Test]
        public void GetHeaders_Always_ReturnHeaderInformation()
        {
            // Arrange
            var headers = new NameValueCollection
            {
                { "key", "value" }
            };

            // Act
            var headerInfo = requestAnalyzer.GetHeaders(headers);

            // Assert
            Assert.AreEqual(headers["key"], headerInfo["key"]);
        }

        [Test]
        public void Analyze_Always_ReturnRequestInfo()
        {
            // Arrange
            var request = new HttpRequest("test", "http://test.com?demo=1", "demoquerystring");

            // Act
            var requestInfo = requestAnalyzer.Analyze(request);

            // Assert
            Assert.AreEqual("?demo=1", requestInfo.QueryString);
            Assert.AreEqual("GET", requestInfo.HttpMethod);
        }
    }
}