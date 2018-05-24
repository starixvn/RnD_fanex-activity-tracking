namespace Fanex.ActivityTracking.Web
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Web;
    using Models;
    using Services;

    public interface IRequestAnalyzer
    {
        RequestInfo Analyze(HttpRequest request);
    }

    public class BaseRequestAnalyzer : IRequestAnalyzer
    {
        private readonly IJsonConverter jsonConverter;

        public BaseRequestAnalyzer(IJsonConverter jsonConverter)
        {
            this.jsonConverter = jsonConverter;
        }

        public RequestInfo Analyze(HttpRequest request)
        {
            return new RequestInfo
            {
                HttpMethod = request.HttpMethod,
                Form = request.Form.ToString(),
                QueryString = request.Url.Query,
                BodyData = GetBodyData(request.InputStream),
                HttpData = jsonConverter.SerializeObject(
                new
                {
                    Headers = GetHeaders(request.Headers)
                })
            };
        }

        internal Dictionary<string, string> GetHeaders(NameValueCollection requestHeaders)
        {
            var headers = new Dictionary<string, string>();

            foreach (var headerKey in requestHeaders.AllKeys)
            {
                headers.Add(headerKey, requestHeaders[headerKey]);
            }

            return headers;
        }

        internal string GetBodyData(Stream requestStream)
        {
            if (requestStream == null)
            {
                return string.Empty;
            }

            string bodyData = GetStreamData(requestStream);

            if (requestStream.CanSeek)
            {
                requestStream.Seek(0, SeekOrigin.Begin);
            }

            if (!string.IsNullOrWhiteSpace(bodyData))
            {
                return HttpUtility.UrlDecode(bodyData);
            }

            return string.Empty;
        }

        private static string GetStreamData(Stream requestStream)
        {
            string bodyData = string.Empty;
            var tempStream = new MemoryStream();

            requestStream.CopyTo(tempStream);

            tempStream.Seek(0, SeekOrigin.Begin);
            using (var streamReader = new StreamReader(tempStream))
            {
                bodyData = streamReader.ReadToEnd();
            }

            return bodyData;
        }
    }
}