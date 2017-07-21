namespace Fanex.ActivityTracking.IntegrationTests.Site.Web
{
    using Services;

    public class JsonConverter : IJsonConverter
    {
        public string SerializeObject(object value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }
    }
}