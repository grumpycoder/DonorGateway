using DonorGateway.Data;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DonorGateway.Admin.Identity
{
    public class ApplicationRoleStore : RoleStore<IdentityRole>
    {
        public ApplicationRoleStore(DataContext context) : base(context)
        {
        }
    }
}