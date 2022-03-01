using Nop.Services.Seo;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Security;
using Nop.Services.Catalog;
using Nop.Services.Topics;
using Nop.Services.Common;

namespace Nop.Services.Customization.Seo
{
    public class SitemapGeneratorOverride : SitemapGenerator
    {
        #region Fields
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        #endregion

        #region Ctor
        public SitemapGeneratorOverride(BlogSettings blogSettings,
            CommonSettings commonSettings,
            ForumSettings forumSettings,
            IActionContextAccessor actionContextAccessor,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductService productService,
            IProductTagService productTagService,
            IStoreContext storeContext,
            ITopicService topicService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            NewsSettings newsSettings,
            SecuritySettings securitySettings) :base(blogSettings,
            commonSettings,
            forumSettings,
            actionContextAccessor,
            categoryService,
            manufacturerService,
            productService,
            productTagService,
            storeContext,
            topicService,
            urlHelperFactory,
            urlRecordService,
            webHelper,
            newsSettings,
            securitySettings)
        {
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
            _productService = productService;
            _storeContext = storeContext;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get category URLs for the sitemap
        /// </summary>
        /// <returns>Sitemap URLs</returns>
        protected override IEnumerable<SitemapUrl> GetCategoryUrls()
        {
            var urlHelper = GetUrlHelper();
            return _categoryService.GetAllCategories().Select(category =>
            {
                var url = UrlStrucutre.UrlDecode(urlHelper.RouteUrl("Category", new { SeName = _urlRecordService.GetSeName(category) }, GetHttpProtocol()));
                return new SitemapUrl(url, UpdateFrequency.Weekly, category.UpdatedOnUtc);
            });
        }

        /// <summary>
        /// Get product URLs for the sitemap
        /// </summary>
        /// <returns>Sitemap URLs</returns>
        protected override IEnumerable<SitemapUrl> GetProductUrls()
        {
            var urlHelper = GetUrlHelper();
            return _productService.SearchProducts(storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true, orderBy: ProductSortingEnum.CreatedOn).Select(product =>
                {
                    var url = UrlStrucutre.UrlDecode(urlHelper.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }, GetHttpProtocol()));
                    return new SitemapUrl(url, UpdateFrequency.Weekly, product.UpdatedOnUtc);
                });
        }
        #endregion
    }
}
