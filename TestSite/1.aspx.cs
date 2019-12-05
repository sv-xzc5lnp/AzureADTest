using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _1 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        if (Request.IsAuthenticated)
        {
            HttpCookie cookie = Request.Cookies.Get(FormsAuthentication.FormsCookieName);
            if (cookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                Response.Write($"{cookie.Value} {Environment.NewLine}");
                var handler = new JwtSecurityTokenHandler();
                var x = handler.ReadToken(ticket.UserData);
                
                var token = new JwtSecurityToken(ticket.UserData);
                Response.Write(token);
            }
        }
    }
}