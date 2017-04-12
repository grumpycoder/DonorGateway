using System.Security.Claims;
using System.Web.Mvc;
using ClaimTypes = System.IdentityModel.Claims.ClaimTypes;

namespace DonorGateway.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.GivenName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.GivenName).Value;
            return View();
        }

        public ActionResult Events()
        {
            ViewBag.GivenName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.GivenName).Value;
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult DonorTax()
        {
            ViewBag.GivenName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.GivenName).Value;
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Mailers()
        {
            ViewBag.GivenName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.GivenName).Value;
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Demographics()
        {
            ViewBag.GivenName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.GivenName).Value;
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}