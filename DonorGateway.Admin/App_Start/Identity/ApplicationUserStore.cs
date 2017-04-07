using DonorGateway.Data;
using DonorGateway.Domain;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DonorGateway.Admin.Identity
{
    public class ApplicationUserStore : UserStore<ApplicationUser>
    {
        public ApplicationUserStore(DataContext context) : base(context)
        {
        }
    }
}