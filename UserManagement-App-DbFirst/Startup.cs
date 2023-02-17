using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(UserManagement_App_DbFirst.Startup))]
namespace UserManagement_App_DbFirst
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
