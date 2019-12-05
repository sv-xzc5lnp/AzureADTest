using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Threading.Tasks;

[assembly: OwinStartupAttribute(typeof(TestSite.Startup))]
namespace TestSite
{
    public partial class Startup {

        
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }

       
    }
}
