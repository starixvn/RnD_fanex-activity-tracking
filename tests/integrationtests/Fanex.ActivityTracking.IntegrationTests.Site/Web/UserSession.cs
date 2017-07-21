namespace Fanex.ActivityTracking.IntegrationTests.Site.Web
{
    using System.Web;
    using Models;

    public class UserSession : IUserSession
    {
        public int AdminId => 1;

        public string AdminName => "AdminDemo";

        public int CustId => 0;

        public string CustName => string.Empty;

        public string SessionID => HttpContext.Current.Session.SessionID;
    }
}