using System;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var model = new SetSessionModel { Key = "CurrentTime", Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(SetSessionModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Session[model.Key] = model.Value;

            ViewBag.Message = "Set Success";

            return View(model);
        }
    }
}