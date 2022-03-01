using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.Eskadenia.ViewEngines;

namespace Nop.Plugin.Payments.Eskadenia.Infrastructure
{
    public class PluginStartup :  INopStartup
    {

        public void Configure(IApplicationBuilder application)
        {
            // Do Nothing Now
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new PaymentsEskadeniaLocationExpander());
            });
        }

        public int Order
        {
            get { return 100000; }
        }
    }
}
