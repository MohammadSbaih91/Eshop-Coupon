using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Checkout;
using Widgets.CustomerOrderReview.Domain;
using Widgets.CustomerOrderReview.Models;

namespace Widgets.CustomerOrderReview.Components
{
    [ViewComponent(Name = "CustomerOrderReview")]
    public class CustomerOrderReviewViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;
        private readonly IRepository<Domain.CustomerOrderReview> _customerOrderReviewRepository;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrderService _orderService;


        public CustomerOrderReviewViewComponent(ISettingService settingService,
            IRepository<Domain.CustomerOrderReview> customerOrderReviewRepository, IWorkContext workContext,
            IStoreContext storeContext, ILocalizationService localizationService,
            IHttpContextAccessor httpContextAccessor,
            IOrderService orderService)
        {
            _settingService = settingService;
            _customerOrderReviewRepository = customerOrderReviewRepository;
            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _httpContextAccessor = httpContextAccessor;
            _orderService = orderService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = new CustomerOrderReviewModel
            {
                Rate1 = 3, Rate2 = 3, Rate3 = 3, Rate4 = 3,
                CustomerOrderReviewType = CustomerOrderReviewType.Positive
            };

            var customerOrderReviewSettings = _settingService.LoadSetting<CustomerOrderReviewSettings>();
            model.Rate1Label = _localizationService.GetLocalizedSetting(customerOrderReviewSettings,
                (x => x.Rate1Label), _workContext.WorkingLanguage.Id, 0);
            model.Rate2Label = _localizationService.GetLocalizedSetting(customerOrderReviewSettings,
                (x => x.Rate2Label), _workContext.WorkingLanguage.Id, 0);
            model.Rate3Label = _localizationService.GetLocalizedSetting(customerOrderReviewSettings,
                (x => x.Rate3Label), _workContext.WorkingLanguage.Id, 0);
            model.Rate4Label = _localizationService.GetLocalizedSetting(customerOrderReviewSettings,
                (x => x.Rate4Label), _workContext.WorkingLanguage.Id, 0);
            model.FeedbackLabel = _localizationService.GetLocalizedSetting(customerOrderReviewSettings,
                (x => x.FeedbackLabel), _workContext.WorkingLanguage.Id, 0);

            var orderId = 0;
            var checkoutModel = additionalData as CheckoutCompletedModel;
            if (checkoutModel != null)
            {
                orderId = checkoutModel.OrderId;
                var order = _orderService.GetOrderById(orderId);
                if (order.PickUpInStore)
                    return Content("");
            }
            else
            {
                int.TryParse(_httpContextAccessor.HttpContext.Request.Query["orderId"], out orderId);
            }
            model.OrderId = orderId;
            model.CustomerId = _workContext.CurrentCustomer.Id;
            model.StoreId = _storeContext.CurrentStore.Id;
            var existingReview = _customerOrderReviewRepository.Table.FirstOrDefault(cr =>
                cr.OrderId == model.OrderId
                && cr.CustomerId == model.CustomerId
                && cr.StoreId == model.StoreId);

            if (existingReview != null)
            {
                model.Rate1 = existingReview.Rate1;
                model.Rate2 = existingReview.Rate2;
                model.Rate3 = existingReview.Rate3;
                model.Rate4 = existingReview.Rate4;
                model.Feedback = existingReview.Feedback;
                model.CustomerOrderReviewType = existingReview.CustomerOrderReviewType;
            }

            return View("~/Plugins/Widgets.CustomerOrderReview/Views/WriteReview.cshtml", model);
        }
    }
}