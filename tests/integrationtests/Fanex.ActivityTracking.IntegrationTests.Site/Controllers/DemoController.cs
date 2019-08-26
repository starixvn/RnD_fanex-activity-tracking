namespace Fanex.ActivityTracking.IntegrationTests.Site.Controllers
{
    using System.Web.Mvc;

    public class DemoController : Controller
    {
        // GET: Demo
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TestTracking()
        {
            return new ContentResult
            {
                Content = "OK"
            };
        }

        public ActionResult TestTracking2()
        {
            return new ContentResult
            {
                Content = "TestTracking2"
            };
        }
    }
}
