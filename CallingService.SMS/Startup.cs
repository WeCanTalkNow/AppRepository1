using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CallingService.SMS.Startup))]
namespace CallingService.SMS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
