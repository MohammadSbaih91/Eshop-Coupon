using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Infrastructure.Extensions;
using Widgets.CustomerOrderReview.Data;

namespace Widgets.CustomerOrderReview.Infrastructure
{
    public class CustomerOrderReviewStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //add object context
            services.AddDbContext<CustomerOrderReviewObjectContext>(optionsBuilder =>
            {
                optionsBuilder.UseSqlServerWithLazyLoading(services);
            });
            
//            services.Configure<RazorViewEngineOptions>(options =>
//            {
//                options.ViewLocationExpanders.Add(new CustomerOrderReviewViewLocationExpander());
//            });
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 10000;
    }
}
