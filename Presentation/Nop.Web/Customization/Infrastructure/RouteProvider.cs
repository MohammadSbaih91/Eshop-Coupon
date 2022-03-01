using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Web.Customization.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        #region Methods

        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //reorder routes so the most used ones are on top. It can improve performance

            //AnonymousOrderTrackByIdByEmail
            routeBuilder.MapLocalizedRoute("AnonymousOrderTrackByIdByEmail", "EShopCommon/anonymousordertrackbyidbyemail/",
                new { controller = "EShopCommon", action = "AnonymousOrderTrackByIdByEmail" });

            routeBuilder.MapLocalizedRoute("EmployeeDetail", "Employee/EmployeeDetail/{orderid}",
                new { controller = "Employee", action = "EmployeeDetail" });

            routeBuilder.MapLocalizedRoute("AddToCompareCategory", "comparecategorypropduct/{categoryId}",
                new { controller = "CustomProduct", action = "AddProductToCompare" });

            routeBuilder.MapLocalizedRoute("AddPackageToCart", "shoppingcart/addpackagetocart/{packageId:min(0)}",
                new { controller = "ShoppingCart", action = "AddPackageToCart" });

            routeBuilder.MapLocalizedRoute("AddProductReview", "/productreview",
                defaults: new { controller = "Product", action = "AddProductReview" });

            routeBuilder.MapLocalizedRoute("UploadIDFile", "UploadIDFile",
               new { controller = "Checkout", action = "UploadIDFile" });

            routeBuilder.MapRoute("sitemap_index.xml", "sitemap_index.xml",
                new { controller = "Common", action = "SitemapIndexedXml" });
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority
        {
            get { return 1; }
        }

        #endregion
    }
}
