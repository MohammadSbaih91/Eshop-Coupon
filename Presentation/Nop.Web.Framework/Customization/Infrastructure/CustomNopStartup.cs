using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Infrastructure.Extensions;
using Nop.Web.Framework.Customization.Mvc.Filters;

namespace Nop.Web.Framework.Customization.Infrastructure
{
    public class CustomNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<CustomActionFilter>();
            });

            services.AddAuthorization();

        }
        public void Configure(IApplicationBuilder application)
        {
           
        }

        public int Order => 11;
    }
}
