using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Security.Claims;
using System.Web;
using System.Web.UI;

public partial class _Default : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Request.IsAuthenticated)
        {
            this.Context.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/Main.aspx" },    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            string value = Request.Form.Get("id_token");

        }
    }
}