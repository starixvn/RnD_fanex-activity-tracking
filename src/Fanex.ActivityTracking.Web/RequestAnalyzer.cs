namespace Fanex.ActivityTracking.Web
{
    using System.Collections.Generic;
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
                BodyData = GetBodyData(request),
                HttpData = jsonConverter.SerializeObject(
                    new
                    {
                        Headers = GetHeaders(request)
                    })
            };
        }

        private Dictionary<string, string> GetHeaders(HttpRequest request)
        {
            var headers = new Dictionary<string, string>();

            foreach (var headerKey in request.Headers.AllKeys)
            {
                headers.Add(headerKey, request.Headers[headerKey]);
            }

            return headers;
        }

        private string GetBodyData(HttpRequest request)
        {
            Stream req = request.InputStream;
            req.Seek(0, SeekOrigin.Begin);
            var streamReader = new StreamReader(req);
            var bodyData = streamReader.ReadToEnd();
            req.Seek(0, SeekOrigin.Begin);
            if (!string.IsNullOrWhiteSpace(bodyData))
            {
                return HttpUtility.UrlDecode(bodyData);
            }

            return string.Empty;
        }
    }
}