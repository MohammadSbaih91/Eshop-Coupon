using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class CustomPriceRangeViewComponent : NopViewComponent
    {
        private readonly ICatalogModelFactory _catalogModelFactory;

        public CustomPriceRangeViewComponent(ICatalogModelFactory catalogModelFactory)
        {
            this._catalogModelFactory = catalogModelFactory;
        }

        public IViewComponentResult Invoke(int currentCategoryId = 0)
        {
            var model = _catalogModelFactory.PreparePriceRangeModel(currentCategoryId);
            if (model.CurrentMinPrice <= 0 && model.CurrentMaxPrice <= 0)
                return Content("");

            return View(model);
        }
    }
}
