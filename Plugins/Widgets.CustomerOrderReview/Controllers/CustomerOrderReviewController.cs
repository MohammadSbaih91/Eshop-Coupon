using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Widgets.CustomerOrderReview.Models;
using Nop.Core.Html;
using Nop.Web.Framework.Mvc.Filters;

namespace Widgets.CustomerOrderReview.Controllers
{
    [AuthorizeAdmin, Area(AreaNames.Admin)]
    public class CustomerOrderReviewController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IRepository<Domain.CustomerOrderReview> _customerOrderReviewRepository;
        private const string PluginPath ="~/Plugins/Widgets.CustomerOrderReview/Views/";

        public CustomerOrderReviewController(
            IPermissionService permissionService,
            ISettingService settingService,
            ILocalizationService localizationService,
            ILanguageService languageService, IDateTimeHelper dateTimeHelper,
            IRepository<Domain.CustomerOrderReview> customerOrderReviewRepository)
        {
            _permissionService = permissionService;
            _settingService = settingService;
            _localizationService = localizationService;
            _languageService = languageService;
            _dateTimeHelper = dateTimeHelper;
            _customerOrderReviewRepository = customerOrderReviewRepository;
        }

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();
            const int storeScope = 0;

            var customerOrderReviewSettings = _settingService.LoadSetting<CustomerOrderReviewSettings>(storeScope);

            var model = new ConfigurationModel
            {
                Rating1 = customerOrderReviewSettings.Rate1Label,
                Rating2 = customerOrderReviewSettings.Rate2Label,
                Rating3 = customerOrderReviewSettings.Rate3Label,
                Rating4 = customerOrderReviewSettings.Rate4Label,
                Feedback = customerOrderReviewSettings.FeedbackLabel,
            };

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Rating1 = this._localizationService.GetLocalizedSetting(
                    customerOrderReviewSettings,
                    (x => x.Rate1Label),
                    languageId, storeScope,
                    false, false);
                locale.Rating2 = this._localizationService.GetLocalizedSetting(
                    customerOrderReviewSettings,
                    (x => x.Rate2Label),
                    languageId, storeScope,
                    false, false);
                locale.Rating3 = this._localizationService.GetLocalizedSetting(
                    customerOrderReviewSettings,
                    (x => x.Rate3Label),
                    languageId, storeScope,
                    false, false);
                locale.Rating4 = this._localizationService.GetLocalizedSetting(
                    customerOrderReviewSettings,
                    (x => x.Rate4Label),
                    languageId, storeScope,
                    false, false);
                locale.Feedback = this._localizationService.GetLocalizedSetting(
                    customerOrderReviewSettings,
                    (x => x.FeedbackLabel),
                    languageId, storeScope,
                    false, false);
            });

            return View($"{PluginPath}Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var customerOrderReviewSettings = _settingService.LoadSetting<CustomerOrderReviewSettings>();
            customerOrderReviewSettings.Rate1Label = model.Rating1;
            customerOrderReviewSettings.Rate2Label = model.Rating2;
            customerOrderReviewSettings.Rate3Label = model.Rating3;
            customerOrderReviewSettings.Rate4Label = model.Rating4;
            customerOrderReviewSettings.FeedbackLabel = model.Feedback;
            foreach (var locale in model.Locales)
            {
                _localizationService.SaveLocalizedSetting(customerOrderReviewSettings, (x => x.Rate1Label),
                    locale.LanguageId, locale.Rating1);
                _localizationService.SaveLocalizedSetting(customerOrderReviewSettings, (x => x.Rate2Label),
                    locale.LanguageId, locale.Rating2);
                _localizationService.SaveLocalizedSetting(customerOrderReviewSettings, (x => x.Rate3Label),
                    locale.LanguageId, locale.Rating3);
                _localizationService.SaveLocalizedSetting(customerOrderReviewSettings, (x => x.Rate4Label),
                    locale.LanguageId, locale.Rating4);
                _localizationService.SaveLocalizedSetting(customerOrderReviewSettings, (x => x.FeedbackLabel),
                    locale.LanguageId, locale.Feedback);
            }

            _settingService.SaveSetting(customerOrderReviewSettings);
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //prepare model
            var model = PrepareCustomerOrderSearchModel(new CustomerOrderReviewSearchModel());

            return View($"{PluginPath}List.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult List(CustomerOrderReviewSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = PrepareCustomerOrderListModel(searchModel);

            return Json(model);
        }


        public virtual CustomerOrderReviewSearchModel PrepareCustomerOrderSearchModel(
            CustomerOrderReviewSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));


//            //prepare available stores
//            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual CustomerOrderReviewListModel PrepareCustomerOrderListModel(
            CustomerOrderReviewSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter reviews
            var createdOnFromValue = !searchModel.CreatedOnFrom.HasValue
                ? null
                : (DateTime?) _dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnFrom.Value,
                    _dateTimeHelper.CurrentTimeZone);
            var createdToFromValue = !searchModel.CreatedOnTo.HasValue
                ? null
                : (DateTime?) _dateTimeHelper
                    .ConvertToUtcTime(searchModel.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);


            //get product reviews
            var orderReviews = GetAllReviews(customerId: 0,
                fromUtc: createdOnFromValue,
                toUtc: createdToFromValue,
                message: searchModel.SearchText,
                orderId: searchModel.SearchOrderId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new CustomerOrderReviewListModel
            {
                Data = orderReviews.Select(orderReview =>
                {
                    //fill in model values from the entity
                    var customerReviewModel = new CustomerOrderReviewModel
                    {
                        Id = orderReview.Id,
                        OrderId = orderReview.OrderId,
                        CustomerId = orderReview.CustomerId,
                        StoreId = orderReview.StoreId,
                        Feedback = orderReview.Feedback,
                        CustomerOrderReviewType = orderReview.CustomerOrderReviewType,
                        Rate1 = orderReview.Rate1,
                        Rate2 = orderReview.Rate2,
                        Rate3 = orderReview.Rate3,
                        Rate4 = orderReview.Rate4,
                        CreatedOn = orderReview.CreatedOnUtc
                    };

                    //convert dates to the user time
                    customerReviewModel.CreatedOn =
                        _dateTimeHelper.ConvertToUserTime(orderReview.CreatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
//                    customerReviewModel.CustomerInfo = orderReview.Customer.IsRegistered()? orderReview.Customer.Email: _localizationService.GetResource("Admin.Customers.Guest");
                    customerReviewModel.Feedback =
                        HtmlHelper.FormatText(orderReview.Feedback, false, true, false, false, false, false);

                    return customerReviewModel;
                }),
                Total = orderReviews.TotalCount
            };

            return model;
        }

        public virtual IPagedList<Domain.CustomerOrderReview> GetAllReviews(int customerId,
            DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = null, int storeId = 0, int orderId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _customerOrderReviewRepository.Table;

            if (customerId > 0)
                query = query.Where(pr => pr.CustomerId == customerId);
            if (fromUtc.HasValue)
                query = query.Where(pr => fromUtc.Value <= pr.CreatedOnUtc);
            if (toUtc.HasValue)
                query = query.Where(pr => toUtc.Value >= pr.CreatedOnUtc);
            if (!string.IsNullOrEmpty(message))
                query = query.Where(pr => pr.Feedback.Contains(message));

            if (orderId > 0)
                query = query.Where(pr => pr.OrderId == orderId);

            if (storeId > 0)
                query = query.Where(pr => pr.StoreId == storeId);

            query = query.OrderBy(pr => pr.CreatedOnUtc).ThenBy(pr => pr.Id);

            var CustomerOrders = new PagedList<Domain.CustomerOrderReview>(query, pageIndex, pageSize);

            return CustomerOrders;
        }
    }
}