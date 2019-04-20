using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Soundify.Startup))]
namespace Soundify
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
