using System.Security.Claims;
using System.Web.Mvc;
using DonorGateway.Admin.Filters;

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

        [AuthorizeEx(Roles = "rsvp")]
        public ActionResult Events()
        {
            return View();
        }

        [AuthorizeEx(Roles = "tax, administrator")]
        public ActionResult DonorTax()
        {
            return View();
        }

        [AuthorizeEx(Roles = "markit, administrator")]
        public ActionResult Mailers()
        {
            return View();
        }

        [AuthorizeEx(Roles = "demographics, administrator")]
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