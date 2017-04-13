using System.Web.Mvc;

namespace DonorGateway.Admin.Filters
{
    public class AuthorizeExAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/unauthorized");
        }
    }
}
