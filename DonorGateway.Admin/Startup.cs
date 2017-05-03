using System.Linq;
using DonorGateway.Admin.Identity;
using DonorGateway.Data;
using DonorGateway.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using Startup = DonorGateway.Admin.Startup;

[assembly: OwinStartup(typeof(Startup))]

namespace DonorGateway.Admin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public static async void Seed()
        {
            var context = DataContext.Create();
            var userManager = new ApplicationUserManager(new ApplicationUserStore(context));
            var roleManager = new ApplicationRoleManager(new ApplicationRoleStore(context));

            string[] roles = { "admin", "user" };
            foreach (var role in roles)
            {
                if (!context.Roles.Any(r => r.Name == role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (!context.Users.Any())
            {
                var user = new ApplicationUser()
                {
                    UserName = "admin",
                    Email = "mark.lawrence@splcenter.org"
                };
                userManager.Create(user, "password");
                userManager.AddToRole(user.Id, "user");
                userManager.AddToRole(user.Id, "admin");
            }

        }
    }
}