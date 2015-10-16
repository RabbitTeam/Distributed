using System.Web.Mvc;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string key)
        {
            ViewBag.Value = Session[key];
            return View();
        }
    }
}