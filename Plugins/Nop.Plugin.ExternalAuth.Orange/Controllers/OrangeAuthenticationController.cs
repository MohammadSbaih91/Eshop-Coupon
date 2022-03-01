using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core.Domain.Customers;
using Nop.Plugin.ExternalAuth.Orange.Models;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services.Authentication;
using System;
using System.Linq;
using Nop.Core;
using Nop.Services.Orders;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Common;

namespace Nop.Plugin.ExternalAuth.Orange.Controllers
{
    public class OrangeAuthenticationController : BasePluginController
    {
        #region Fields

        private readonly OrangeExternalAuthSettings _orangeExternalAuthSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        #endregion

        #region Ctor

        public OrangeAuthenticationController(OrangeExternalAuthSettings orangeExternalAuthSettings,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            ICustomerService customerService,
            CustomerSettings customerSettings,
            ICustomerRegistrationService customerRegistrationService,
            IAuthenticationService authenticationService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            IEventPublisher eventPublisher,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            ILogger logger)
        {
            _orangeExternalAuthSettings = orangeExternalAuthSettings;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _customerService = customerService;
            _customerSettings = customerSettings;
            _customerRegistrationService = customerRegistrationService;
            _authenticationService = authenticationService;
            _storeContext = storeContext;
            _workContext = workContext;
            _shoppingCartService = shoppingCartService;
            _eventPublisher = eventPublisher;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
        }

        #endregion

        #region Utilities
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        protected void LoginSSO(Customer customer)
        {
            // migrate shopping cart
            _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer, true);

            //sign in new customer
            _authenticationService.SignIn(customer, false);

            //raise event       
            _eventPublisher.Publish(new CustomerLoggedinEvent(customer));

            //activity log
            _customerActivityService.InsertActivity(customer, "PublicStore.Login",
                _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);
        }
        #endregion

        #region Methods
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                OrnageURL = _orangeExternalAuthSettings.OrnageURL,
                IsEnable = _orangeExternalAuthSettings.IsEnable,
            };

            return View("~/Plugins/ExternalAuth.Orange/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _orangeExternalAuthSettings.OrnageURL = model.OrnageURL;
            _orangeExternalAuthSettings.IsEnable = model.IsEnable;
            _settingService.SaveSetting(_orangeExternalAuthSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        public IActionResult ExternalAuthOrangeLogin(string JWT)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                //_logger.InsertLog(Core.Domain.Logging.LogLevel.Debug, "JWT TOKEN", JWT);
                if (!string.IsNullOrEmpty(JWT))
                {
                    var jsonToken = handler.ReadToken(JWT);
                    var tokenS = jsonToken as JwtSecurityToken;
                    string json = JsonConvert.SerializeObject(tokenS.Payload);
                    JWTResponseModel model = JsonConvert.DeserializeObject<JWTResponseModel>(json);

                    var email = !string.IsNullOrEmpty(model.AlternativEmail) ? model.AlternativEmail : model.Username;
                    if (!email.Contains('@'))
                    {
                        email += "@eshop.com";
                    }

                    var customer = _customerService.GetCustomerByEmailOrUser(model.Username, email);
                    if (customer != null)
                    {
                        LoginSSO(customer);

                        if (string.IsNullOrEmpty(model.RedirectUrl))
                            return RedirectToRoute("HomePage");

                        return Redirect(model.RedirectUrl);
                    }
                    else
                    {
                        customer = _workContext.CurrentCustomer;
                        customer.RegisteredInStoreId = _storeContext.CurrentStore.Id;
                        var registrationRequest = new CustomerRegistrationRequest(customer,
                            email,
                            model.Username,
                            RandomString(8),
                            _customerSettings.DefaultPasswordFormat,
                            _storeContext.CurrentStore.Id,
                            true);

                        var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
                        if (registrationResult.Success)
                        {
                            LoginSSO(customer);

                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FirstNameAttribute, model.FName);
                            _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.LastNameAttribute, model.LName);

                            if (string.IsNullOrEmpty(model.RedirectUrl))
                                return RedirectToRoute("HomePage");

                            return Redirect(model.RedirectUrl);
                        }
                        else
                        {
                            ErrorNotification(string.Join(":", registrationResult.Errors));
                            return RedirectToRoute("Login");
                        }
                    }
                }
                else
                {
                    return RedirectToRoute("Login");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("JWT", ex);
                return RedirectToRoute("Login");
            }
        }
        #endregion
    }
}