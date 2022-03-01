using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Services.Messages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Localization;

namespace Nop.Web.Components
{
    public class NewsletterBoxViewComponent : NopViewComponent
    {
        private readonly CustomerSettings _customerSettings;
        private readonly INewsletterModelFactory _newsletterModelFactory;
        private readonly INewsLetterSubscriptionTypeService _newsLetterSubscriptionTypeService;
        private readonly ILocalizationService _localizationService;

        public NewsletterBoxViewComponent(CustomerSettings customerSettings, INewsletterModelFactory newsletterModelFactory,
            INewsLetterSubscriptionTypeService newsLetterSubscriptionTypeService,
            ILocalizationService localizationService)
        {
            this._customerSettings = customerSettings;
            this._newsletterModelFactory = newsletterModelFactory;
            this._newsLetterSubscriptionTypeService = newsLetterSubscriptionTypeService;
            this._localizationService = localizationService;
        }

        public IViewComponentResult Invoke()
        {
            if (_customerSettings.HideNewsletterBlock)
                return Content("");

            var model = _newsletterModelFactory.PrepareNewsletterBoxModel();
            var newsLatterTypes = _newsLetterSubscriptionTypeService.GetNewsLetterSubscriptionTypes();
            foreach (var newsLatterType in newsLatterTypes)
            {
                model.AvailableNewsLetterSubscriptionTypes.Add(new SelectListItem()
                {
                    Text = _localizationService.GetLocalized(newsLatterType, x => x.Name),
                    Value = newsLatterType.Id.ToString()
                });
            }
            return View(model);
        }
    }
}
