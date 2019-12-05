using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestSite
{
    public partial class Startup {

        public static string clientId = "776d48fd-ad0a-46ae-9bcc-b28dc34a2ed8";       
        public static string redirectUri = "http://localhost/UATSL/Main.aspx";       
        public static string tenant = "74179e4d-962e-4fdb-bd11-9803ffe026cb";       
        string authority = "http://login.microsoftonline.com/74179e4d-962e-4fdb-bd11-9803ffe026cb/oauth2/authorize";

       
        public void ConfigureAuth(IAppBuilder app)
        {
            IdentityModelEventSource.ShowPII = true;
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {

                    ClientId = clientId,
                    Authority = authority,
                    RedirectUri = redirectUri,
                    ClientSecret= "=EvIaZpfxCK=@.tLB63WMd2P5BC2Ax=P",
                    PostLogoutRedirectUri = redirectUri,
                    Scope = OpenIdConnectScope.OpenIdProfile,
                    BackchannelHttpHandler = new HttpClientHandler
                    {
                        UseProxy = true,
                        Proxy = new WebProxy
                        {
                            Address = new Uri(""),
                            Credentials = new NetworkCredential("gscfgfgfdgfgfdgfdgpro", "fdgkjfgfdgjfdkjg")
                        }
                    },
                    ResponseType = OpenIdConnectResponseType.IdToken,
                    MetadataAddress= "https://login.microsoftonline.com/74179e4d-962e-4fdb-bd11-9803ffe026cb/v2.0/.well-known/openid-configuration",
                    RequireHttpsMetadata=true,
                    TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = false // Simplification (see note below)
                    },
                   
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthenticationFailed = OnAuthenticationFailed,
                        AuthorizationCodeReceived = OnAuthorizationCodeReceived,
                        SecurityTokenValidated = OnSecurityTokenValidatedAsync
                    }
                }
            );
            
           
        }

        private Task OnSecurityTokenValidatedAsync(SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            // Make sure that the user didn't sign in with a personal Microsoft account
            if (notification.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value == tenant)
            {
                notification.HandleResponse();
                notification.Response.Redirect("/Account/UserMismatch");
            }

            return Task.FromResult(0);
        }

        private Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification context)
        {
            context.HandleResponse();
            var token = context.JwtSecurityToken.ToString();
            context.Response.Redirect(string.Format("/Main.aspx?Token={0}", token));
            return Task.FromResult(0);
        }

        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            context.HandleResponse();
            context.Response.Redirect("/?errormessage=" + context.Exception.Message);
            return Task.FromResult(0);
        }
    }
}
