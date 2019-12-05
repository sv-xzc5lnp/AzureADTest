<%@ Application Language="C#" %>
<%@ Import Namespace="TestSite" %>
<%@ Import Namespace="System.Web.Optimization" %>
<%@ Import Namespace="System.Web.Routing" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.IdentityModel.Tokens.Jwt" %>

<%@ Import Namespace="System.Security.Claims" %>
<%@ Import Namespace="Microsoft.Identity.Client" %>
<%@ Import Namespace="System.Web.Security" %>
<%@ Import Namespace="System.Collections.Generic" %>

<script runat="server">

    private bool hasToken = false;
    private string token = string.Empty;
    private string upn = string.Empty;
    private string exp = string.Empty;
    void Application_Start(object sender, EventArgs e)
    {
        RouteConfig.RegisterRoutes(RouteTable.Routes);
        BundleConfig.RegisterBundles(BundleTable.Bundles);
    }

    void Application_BeginRequest(object sender, EventArgs e)
    {
        var azuretoken = Request.Form["id_token"];
        hasToken = false;
        if (azuretoken !=null)
        {
            token = azuretoken;
            hasToken = true;
        }
    }

    void Application_AuthenticateRequest(object sender, EventArgs e)
    {

        if (!Request.IsAuthenticated && hasToken)
        {
            IConfidentialClientApplication daemonClient;
            daemonClient = ConfidentialClientApplicationBuilder.Create(TestSite.Startup.clientId)
                .WithAuthority("https://login.microsoftonline.com/74179e4d-962e-4fdb-bd11-9803ffe026cb/v2.0")
                .WithRedirectUri(TestSite.Startup.redirectUri)
                .WithHttpClientFactory(new ClientFactory())
                .WithClientSecret("=EvIaZpfxCK=@.tLB63WMd2P5BC2Ax=P")
                .Build();
            //AuthenticationResult authResult = daemonClient.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" }).ExecuteAsync().GetAwaiter().GetResult();
            //string token = authResult.AccessToken;

            var msaluserToken= CreateCookie(daemonClient, token);

            Response.Redirect("~/Main.aspx");
        }


    }

    private Msdal.MSALUserTokenMemoryCache CreateCookie(IConfidentialClientApplication daemonClient, string token)
    {
        JwtSecurityToken id_token = JwtDecode(token);
        SetUserPrincipal(id_token);
        var exptime = long.Parse(exp);
        var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime expireTime = dt.AddSeconds(exptime);

        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, upn, DateTime.UtcNow, expireTime, false, id_token.ToString(), FormsAuthentication.FormsCookiePath);
        string encryptedCookie = FormsAuthentication.Encrypt(ticket);
        HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedCookie);
        FormsAuthentication.SetAuthCookie(upn, true);

        cookie.Expires = expireTime;
        Request.Cookies.Add(cookie);

        Response.Cookies.Add(cookie);
        var serializedAppTokenCache = new Msdal.MSALUserTokenMemoryCache(TestSite.Startup.clientId, daemonClient.AppTokenCache);
        var serializedUserTokenCache = new Msdal.MSALUserTokenMemoryCache(TestSite.Startup.clientId, daemonClient.UserTokenCache);
        return serializedAppTokenCache;
    }

    private JwtSecurityToken JwtDecode(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ReadToken(token) as JwtSecurityToken;
    }

    private  void SetUserPrincipal(JwtSecurityToken id_token)
    {
        upn = id_token.Claims.Where(c => c.Type == "name").Select(c => c.Value).SingleOrDefault();
        exp = id_token.Claims.Where(c => c.Type == "exp").Select(c => c.Value).SingleOrDefault();

        var name = id_token.Claims.Where(c => c.Type == "preferred_name").Select(c => c.Value).SingleOrDefault();
        var aio = id_token.Claims.Where(c => c.Type == "aio").Select(c => c.Value).SingleOrDefault();
        var iss = id_token.Claims.Where(c => c.Type == "iss").Select(c => c.Value).SingleOrDefault();
        var nbf = id_token.Claims.Where(c => c.Type == "nbf").Select(c => c.Value).SingleOrDefault();

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

</script>
