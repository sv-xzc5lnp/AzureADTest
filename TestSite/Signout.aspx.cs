using Microsoft.Identity.Client;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Msdal;

public partial class Signout : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        this.Context.GetOwinContext().Authentication.SignOut(OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
    }


    private void RemovedCachedTokensForApp()
    {
        string tenantId = ClaimsPrincipal.Current?.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

        if (string.IsNullOrEmpty(tenantId))
        {
            return;
        }

        IConfidentialClientApplication daemonClient;
        daemonClient = ConfidentialClientApplicationBuilder.Create(TestSite.Startup.clientId)
            .WithAuthority("http://login.microsoftonline.com/74179e4d-962e-4fdb-bd11-9803ffe026cb/oauth2/authorize")
            .WithRedirectUri(TestSite.Startup.redirectUri)
          
            .Build();

        var serializedUserTokenCache = new MSALUserTokenMemoryCache(TestSite.Startup.clientId, daemonClient.UserTokenCache);
        serializedUserTokenCache.Clear();
    }
}