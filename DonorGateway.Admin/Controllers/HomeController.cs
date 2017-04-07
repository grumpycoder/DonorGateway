using System.Web.Mvc;

namespace DonorGateway.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Events()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult DonorTax()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Mailers()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Demographics()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}