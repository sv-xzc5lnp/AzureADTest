using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
using System.Security.Claims;
using Microsoft.Identity.Client;
using static Msdal;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Web.Security;

public partial class Main : System.Web.UI.Page
{
    private const string MSGraphScope = "https://graph.microsoft.com/.default";
    private const string MSGraphQuery = "https://graph.microsoft.com/v1.0/users";
    private string upn = string.Empty;
    private string exp = string.Empty;

    protected void  Page_Load(object sender, EventArgs e)
    {
       
        if (this.Context.GetOwinContext().Authentication.User.Identity.IsAuthenticated)
        {
            Response.Write("authenticated");           
        }
        else
        {
            
        }
        //IConfidentialClientApplication daemonClient;
        //daemonClient = ConfidentialClientApplicationBuilder.Create(TestSite.Startup.clientId)
        //    .WithAuthority("https://login.microsoftonline.com/74179e4d-962e-4fdb-bd11-9803ffe026cb/v2.0")
        //    .WithRedirectUri(TestSite.Startup.redirectUri)
        //    .WithHttpClientFactory(new ClientFactory())
        //    .WithClientSecret("=EvIaZpfxCK=@.tLB63WMd2P5BC2Ax=P")
        //    .Build();
        //AuthenticationResult authResult = daemonClient.AcquireTokenForClient(new[] { MSGraphScope }).ExecuteAsync().GetAwaiter().GetResult();
        //Response.Write($"token : {authResult.AccessToken}\n");
        //string token = authResult.AccessToken;
        //MSALUserTokenMemoryCache serializedAppTokenCache =
      
            //CreateCookie(null, this.Session["id_token"].ToString());
        //Getusers(authResult, serializedAppTokenCache);
    }

    private void CreateCookie(IConfidentialClientApplication daemonClient, string token)
    {
        JwtSecurityToken id_token = JwtDecode(token);
        // UserPrincipalNme, ie a fancy word for the original e-mail address you have in ActiveDirectory
        string upn = id_token.Claims.Where(c => c.Type == "preferred_name").Select(c => c.Value).SingleOrDefault();
        var exptime = long.Parse(id_token.Claims.Where(c => c.Type == "exp").Select(c => c.Value).SingleOrDefault());
        var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime expireTime = dt.AddSeconds(exptime);
        SetUserPrincipal(id_token);

        // create the cookie and store the JWT token in the UserData attrribute so we can pick it up 
        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, upn, DateTime.UtcNow, expireTime, false, id_token.ToString(), FormsAuthentication.FormsCookiePath);
        string encryptedCookie = FormsAuthentication.Encrypt(ticket);
        HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedCookie);
        cookie.Expires = expireTime;
        FormsAuthentication.SetAuthCookie(upn, true);
        Response.Cookies.Add(cookie);
        //var serializedAppTokenCache = new MSALUserTokenMemoryCache(TestSite.Startup.clientId, daemonClient.AppTokenCache);
        //var serializedUserTokenCache = new MSALUserTokenMemoryCache(TestSite.Startup.clientId, daemonClient.UserTokenCache);
        //return serializedAppTokenCache;
    }

    private void Getusers(AuthenticationResult authResult, MSALUserTokenMemoryCache serializedAppTokenCache)
    {
        var handler = new HttpClientHandler
        {
            UseProxy = true,
            Proxy = new WebProxy
            {
                Address = new Uri(""),
                Credentials = new NetworkCredential("gsddsdgdgd", "dfgfdgsfdgfdgfd")
            }
        };
        HttpClient client = new HttpClient(handler, true);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, MSGraphQuery);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();       
        string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        Response.Write($"{json}\n");
    }

    private JwtSecurityToken JwtDecode(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ReadToken(token) as JwtSecurityToken;
    }

    private void SetUserPrincipal(JwtSecurityToken id_token)
    {

        upn = id_token.Claims.Where(c => c.Type == "name").Select(c => c.Value).SingleOrDefault();
        var name = id_token.Claims.Where(c => c.Type == "preferred_name").Select(c => c.Value).SingleOrDefault();
        var aio = id_token.Claims.Where(c => c.Type == "aio").Select(c => c.Value).SingleOrDefault();
        var iss = id_token.Claims.Where(c => c.Type == "iss").Select(c => c.Value).SingleOrDefault();
        var nbf = id_token.Claims.Where(c => c.Type == "nbf").Select(c => c.Value).SingleOrDefault();
        exp = id_token.Claims.Where(c => c.Type == "exp").Select(c => c.Value).SingleOrDefault();
        var oid = id_token.Claims.Where(c => c.Type == "oid").Select(c => c.Value).SingleOrDefault();
        var tenantId = id_token.Claims.Where(c => c.Type == "tid").Select(c => c.Value).SingleOrDefault();
        var ver = id_token.Claims.Where(c => c.Type == "ver").Select(c => c.Value).SingleOrDefault();
        List<Claim> claims = new List<Claim>
                {
                     new Claim(ClaimTypes.Name,name??upn)
                    ,new Claim( "http://schemas.microsoft.com/identity/claims/objectidentifier", oid )
                    , new Claim(ClaimTypes.Upn, upn )
                    , new Claim("aio",aio )
                    , new Claim("iss",  iss)
                    , new Claim("nbf", nbf )
                    , new Claim("exp", exp)
                    , new Claim("http://schemas.microsoft.com/identity/claims/tenantid", tenantId)
                    , new Claim("ver",  ver)
                };
        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");

        ClaimsPrincipal principal = new ClaimsPrincipal(claimsIdentity);
        HttpContext.Current.User = principal;
        System.Threading.Thread.CurrentPrincipal = principal; // updates ClaimsPrincipal.Current
    }
}