using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CallingService.Voice.Startup))]
namespace CallingService.Voice
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
