using System.Configuration;
using DonorGateway.Admin.Identity;
using DonorGateway.Data;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

namespace DonorGateway.Admin
{
    public partial class Startup
    {
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }

        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        public static readonly string Authority = aadInstance + tenantId;

        // This is the resource ID of the AAD Graph API.  We'll need this to request a token to call the Graph API.
        string graphResourceId = "https://graph.windows.net";

        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);

            app.CreatePerOwinContext(DataContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = Authority,
                    PostLogoutRedirectUri = postLogoutRedirectUri
                });
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    Audience = "https://developertenant.onmicrosoft.com/WebUXplusAPI",
                    Tenant = "developertenant.onmicrosoft.com",
                    AuthenticationType = "OAuth2Bearer",
                });


            //OAuthBearerOptions = new OAuthBearerAuthenticationOptions();
            //app.UseOAuthBearerAuthentication(OAuthBearerOptions);

            //app.UseWindowsAzureActiveDirectoryBearerAuthentication(new WindowsAzureActiveDirectoryBearerAuthenticationOptions
            //{
            //    Audience = clientId,
            //    Tenant = tenantId

            //});

            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{

            //    AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
            //    LoginPath = new PathString("/Auth/Login"),
            //    Provider = new CookieAuthenticationProvider
            //    {
            //        OnApplyRedirect = ctx =>
            //        {
            //            if (!ctx.Request.Path.StartsWithSegments(new PathString("/api")))
            //            {
            //                ctx.Response.Redirect(ctx.RedirectUri);
            //            }
            //        },
            //        // Enables the application to validate the security stamp when the user logs in.
            //        // This is a security feature which is used when you change a password or add an external login to your account.
            //        OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
            //   validateInterval: TimeSpan.FromMinutes(30),
            //   regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
            //    }
            //});

            //app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            //app.UseCookieAuthentication(new CookieAuthenticationOptions());

            //app.UseWindowsAzureActiveDirectoryBearerAuthentication(new WindowsAzureActiveDirectoryBearerAuthenticationOptions
            //{
            //    Audience = clientId,
            //    Tenant = tenantId,
            //    AuthenticationType = "OAuth2Bearer",
            //});


            //app.UseOpenIdConnectAuthentication(
            //   new OpenIdConnectAuthenticationOptions
            //   {
            //       ClientId = clientId,
            //       Authority = Authority,

            //       PostLogoutRedirectUri = postLogoutRedirectUri,

            //       Notifications = new OpenIdConnectAuthenticationNotifications()
            //       {
            //           // If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
            //           AuthorizationCodeReceived = (context) =>
            //           {
            //               var code = context.Code;
            //               ClientCredential credential = new ClientCredential(clientId, appKey);
            //               string signedInUserId = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
            //               AuthenticationContext authContext = new AuthenticationContext(Authority, new ADALTokenCache(signedInUserId));
            //               AuthenticationResult result = authContext.AcquireTokenByAuthorizationCode(
            //                code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, graphResourceId);

            //               var identity = context.AuthenticationTicket.Identity;
            //               identity.AddClaim(new Claim("id_token", result.IdToken));

            //               identity.AddClaim(new Claim("access_token", result.AccessToken));

            //               context.AuthenticationTicket = new AuthenticationTicket(identity, context.AuthenticationTicket.Properties);

            //               return Task.FromResult(0);
            //           },
            //           SecurityTokenValidated = (context) =>
            //           {
            //               //var identity = context.AuthenticationTicket.Identity; 
            //               //identity.AddClaim(new Claim("id_token", context.ProtocolMessage.IdToken));

            //               //identity.AddClaim(new Claim("access_token", context.ProtocolMessage.AccessToken));

            //               //context.AuthenticationTicket= new AuthenticationTicket(identity, context.AuthenticationTicket.Properties);

            //               return Task.FromResult(0);
            //           },
            //           AuthenticationFailed = (context) =>
            //           {
            //               //this section added to handle scenario where user logs in, but cancels consenting to rights to read directory profile
            //               string appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;

            //               context.ProtocolMessage.RedirectUri = appBaseUrl + "/";

            //               //this is where the magic happens

            //               context.HandleResponse();

            //               context.Response.Redirect(context.ProtocolMessage.RedirectUri);

            //               return Task.FromResult(0);

            //           }
            //       }
            //   });


        }

        private static bool IsAjaxRequest(IOwinRequest request)
        {
            IReadableStringCollection query = request.Query;
            if ((query != null) && (query["X-Requested-With"] == "XMLHttpRequest"))
            {
                return true;
            }
            IHeaderDictionary headers = request.Headers;
            return ((headers != null) && (headers["X-Requested-With"] == "XMLHttpRequest"));
        }

    }
}