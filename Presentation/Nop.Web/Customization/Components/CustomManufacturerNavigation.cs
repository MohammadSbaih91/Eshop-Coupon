using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class CustomManufacturerNavigationViewComponent : NopViewComponent
    {
        private readonly ICatalogModelFactory _catalogModelFactory;

        public CustomManufacturerNavigationViewComponent(ICatalogModelFactory catalogModelFactory)
        {
            this._catalogModelFactory = catalogModelFactory;
        }

        public IViewComponentResult Invoke(int categoryid = 0)
        {
            var model = _catalogModelFactory.PrepareAllManufacturerNavigationModel(categoryid);
            if (!model.Manufacturers.Any())
                return Content("");

            return View(model);
        }
    }
}
