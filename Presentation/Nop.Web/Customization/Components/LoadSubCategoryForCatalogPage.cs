using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Customization.Components
{
    public class LoadSubCategoryForCatalogPageViewComponent : NopViewComponent
    {
        private readonly ICatalogModelFactory _catalogModelFactory;

        public LoadSubCategoryForCatalogPageViewComponent(ICatalogModelFactory catalogModelFactory)
        {
            this._catalogModelFactory = catalogModelFactory;
        }

        public IViewComponentResult Invoke(int currentCategoryId)
        {
            if (currentCategoryId == 0)
                return Content("");

            var model = _catalogModelFactory.PrepareCustomCategoryNavigationModel(currentCategoryId);
            return View(model);
        }
    }
}
