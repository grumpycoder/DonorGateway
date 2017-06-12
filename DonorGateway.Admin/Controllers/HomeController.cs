using System.Security.Claims;
using System.Web.Mvc;

namespace DonorGateway.Admin.Controllers
{

    public class HomeController : Controller
    {
        public HomeController()
        {
            if (ClaimsPrincipal.Current.Identity.IsAuthenticated) ViewBag.GivenName = ClaimsPrincipal.Current.FindFirst("name").Value;

        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Events()
        {
            return View();
        }

        [Authorize]
        public ActionResult DonorTax()
        {
            return View();
        }

        [Authorize]
        public ActionResult Mailers()
        {
            return View();
        }

        [Authorize]
        public ActionResult Demographics()
        {
            return View();
        }

        public ActionResult Unauthorized()
        {
            return View();
        }
    }
}