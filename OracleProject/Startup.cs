using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OracleProject.Startup))]
namespace OracleProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
