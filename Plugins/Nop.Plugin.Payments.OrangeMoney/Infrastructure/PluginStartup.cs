using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.OrangeMoney.ViewEngines;

namespace Nop.Plugin.Payments.OrangeMoney.Infrastructure
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
                options.ViewLocationExpanders.Add(new PaymentsOrangeMoneyLocationExpander());
            });
        }

        public int Order
        {
            get { return 100000; }
        }
    }
}
