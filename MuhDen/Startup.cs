using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MuhDen.Startup))]
namespace MuhDen
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
