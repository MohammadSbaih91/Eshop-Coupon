using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Themes;

namespace Nop.Web.Customization.infrastructure
{
    public class CustomStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new CustomizationViewLocationExpander());
            });
        }

        public void Configure(IApplicationBuilder application)
        {
            var config = EngineContext.Current.Resolve<NopConfig>();

            application.UseStaticFiles(new StaticFileOptions()
            {
                //FileProvider = new PhysicalFileProvider(config.SharedFileStorageContainerName + "/" + Services.Media.NopMediaDefaults.ImageThumbsPath),
                FileProvider = new PhysicalFileProvider(config.SharedFileStorageContainerName + "/" + "images"),
                RequestPath = new PathString("/app-images")
            });
        }

        public int Order => 10000;
    }
}