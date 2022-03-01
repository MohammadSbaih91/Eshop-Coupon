using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Framework.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace Nop.Plugin.Payments.Eskadenia
{
    /// <summary>
    /// Eskadenia payment processor
    /// </summary>
    public class EskadeniaPaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICurrencyService _currencyService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly EskadeniaPaymentSettings _eskadeniaPaymentSettings;
        private readonly IRepository<Language> _languagerepository;
        private readonly INopFileProvider _fileProvider;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IStoreContext _storeContext;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly WidgetSettings _widgetSettings;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public EskadeniaPaymentProcessor(CurrencySettings currencySettings,
            ICurrencyService currencyService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            ITaxService taxService,
            IWebHelper webHelper,
            EskadeniaPaymentSettings eskadeniaPaymentSettings,
            IRepository<Language> languagerepository,
            INopFileProvider fileProvider,
            IOrderService orderService,
            ILogger logger,
            ICustomerService customerService,
            IWorkContext workContext,
            IStoreService storeService,
            IUrlHelperFactory urlHelperFactory,
            IStoreContext storeContext,
            IActionContextAccessor actionContextAccessor,
            WidgetSettings widgetSettings,
            IGenericAttributeService genericAttributeService
            )
        {
            this._currencySettings = currencySettings;
            this._currencyService = currencyService;
            this._httpContextAccessor = httpContextAccessor;
            this._localizationService = localizationService;
            this._paymentService = paymentService;
            this._settingService = settingService;
            this._taxService = taxService;
            this._webHelper = webHelper;
            this._eskadeniaPaymentSettings = eskadeniaPaymentSettings;
            this._languagerepository = languagerepository;
            this._fileProvider = fileProvider;
            this._orderService = orderService;
            this._logger = logger;
            this._customerService = customerService;
            this._workContext = workContext;
            this._storeService = storeService;
            this._urlHelperFactory = urlHelperFactory;
            this._storeContext = storeContext;
            this._actionContextAccessor = actionContextAccessor;
            this._widgetSettings = widgetSettings;
            this._genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Install resource string
        /// </summary>
        protected virtual void InstallLocaleResources()
        {
            //'English' language
            var enLanguage = _languagerepository.Table.FirstOrDefault(l => l.LanguageCulture.ToLower() == "en-us" & l.Published);
            //save resources
            if (enLanguage != null)
            {
                foreach (var filePath in Directory.EnumerateFiles(_fileProvider.MapPath("~/Plugins/Payments.Eskadenia/Localization/ResourceString"),
                "ResourceString_EN.xml", SearchOption.TopDirectoryOnly))
                {
                    var localesXml = File.ReadAllText(filePath);
                    var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                    localizationService.ImportResourcesFromXml(enLanguage, localesXml);
                }
            }

        }

        ///<summry>
        ///Delete Resource String
        ///</summry>
        protected virtual void DeleteLocalResources()
        {
            var file = Path.Combine(_fileProvider.MapPath("~/Plugins/Payments.Eskadenia/Localization/ResourceString"), "ResourceString_EN.xml");
            var languageResourceNames = from name in XDocument.Load(file).Document.Descendants("LocaleResource")
                                        select name.Attribute("Name").Value;

            foreach (var item in languageResourceNames)
            {
                _localizationService.DeletePluginLocaleResource(item);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            // Call ValidateCheckout
            bool isCheckoutValidate = false;
            string checkoutValidateResponse = string.Empty;

            #region Create postfix string

            Guid guid = Guid.NewGuid();
            string randomString = guid.ToString().Replace("-", "").Substring(0, 10);
            _genericAttributeService.SaveAttribute<string>(postProcessPaymentRequest.Order, EskadeniaHelper.ORDER_POSTFIX_STRING, randomString);

            #endregion

            #region 1. Validate checkout

            try
            {
                //create common query parameters for the request
                var validateCartQueryParameters = CreateQueryParameters("validateCheckout", postProcessPaymentRequest);

                // now need to call PayChyeck API for check payment status
                string validateRequestUrl = _eskadeniaPaymentSettings.APIEndPoint + "PayPrep?";

                var requestUrl = QueryHelpers.AddQueryString(validateRequestUrl, validateCartQueryParameters);
                //http://10.1.11.162:2424/PaymentHub/PayPrep?merchantCode=COM-CC-05&channelCode=303331&serviceType=validateCheckout&merchantTrxNo=9999123&payAmount=66&currencyCode=JOD

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUrl);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    checkoutValidateResponse = streamReader.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(checkoutValidateResponse))
                    {
                        var respParameters = checkoutValidateResponse.Replace("--", "").Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var resp in respParameters)
                        {
                            string[] re = resp.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (re.Any())
                            {
                                string key = re[0];
                                if (key.ToLower().Equals("trxstatus"))
                                {
                                    string value = string.Empty;
                                    if (re.Length > 1)
                                    {
                                        value = re[1];
                                        if (value.EndsWith("--"))
                                            value = value.Substring(0, value.Length - 2);

                                        // check status
                                        if (value == StatusCode.OPC00000)
                                        {
                                            isCheckoutValidate = true;
                                            postProcessPaymentRequest.Order.OrderNotes.Add(new OrderNote()
                                            {
                                                Note = "Checkout validation success. Status code- " + value + ", Response - " + checkoutValidateResponse,
                                                OrderId = postProcessPaymentRequest.Order.Id,
                                                DisplayToCustomer = false,
                                                CreatedOnUtc = DateTime.UtcNow
                                            });

                                            _orderService.UpdateOrder(postProcessPaymentRequest.Order);
                                        }
                                        else
                                        {
                                            postProcessPaymentRequest.Order.OrderNotes.Add(new OrderNote()
                                            {
                                                Note = "Checkout validate failed. Status code- " + value + ", Response - " + checkoutValidateResponse,
                                                OrderId = postProcessPaymentRequest.Order.Id,
                                                DisplayToCustomer = false,
                                                CreatedOnUtc = DateTime.UtcNow
                                            });

                                            _orderService.UpdateOrder(postProcessPaymentRequest.Order);

                                            isCheckoutValidate = false;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error on validate checkout- " + ex.Message, ex);
            }

            if (!isCheckoutValidate)
            {
                var checkoutCompleteURL = RouteUrl(routeName: "CheckoutCompleted", routeValues: new { orderId = postProcessPaymentRequest.Order.Id });
                _httpContextAccessor.HttpContext.Response.Redirect(checkoutCompleteURL);
                return;
            }

            #endregion

            #region 2. Redirect to payment page

            //create common query parameters for the proceedCheckout request
            var queryParameters = CreateQueryParameters("proceedCheckout", postProcessPaymentRequest);

            // now need to redirect user to payment gateway portal for proceed ckeckout 
            string checkouProceedtUrl = _eskadeniaPaymentSettings.APIEndPoint + "PayPrep?";

            var paymentRequestUrl = QueryHelpers.AddQueryString(checkouProceedtUrl, queryParameters);
            // http://10.1.11.162:2424/PaymentHub/PayPrep?merchantCode=COM-CC-05&channelCode=303331&serviceType=proceedCheckout&custEmail=qbs@mail.com&custNum=qbs&merchantTrxNo=9999123&payAmount=66&currencyCode=JOD&PSP=2&callBackUrl=http://localhost:15536/PaymentEskadenia/PaymentResponse

            _httpContextAccessor.HttpContext.Response.Redirect(paymentRequestUrl);

            #endregion
        }

        protected virtual string RouteUrl(int storeId = 0, string routeName = null, object routeValues = null)
        {
            //try to get a store by the passed identifier
            var store = _storeService.GetStoreById(storeId) ?? _storeContext.CurrentStore
                ?? throw new Exception("No store could be loaded");

            //ensure that the store URL is specified
            if (string.IsNullOrEmpty(store.Url))
                throw new Exception("URL cannot be null");

            //generate a URL with an absolute path
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = new PathString(urlHelper.RouteUrl(routeName, routeValues));

            //remove the application path from the generated URL if exists
            var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
            url.StartsWithSegments(pathBase, out url);

            //compose the result
            return Uri.EscapeUriString(WebUtility.UrlDecode($"{store.Url.TrimEnd('/')}{url}"));
        }

        /// <summary>
        /// Create common query parameters for the request
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Created query parameters</returns>
        private IDictionary<string, string> CreateQueryParameters(string serviceType, PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            var randomString = _genericAttributeService.GetAttribute<string>(order, EskadeniaHelper.ORDER_POSTFIX_STRING);
            if (string.IsNullOrWhiteSpace(randomString))
            {
                Guid guid = Guid.NewGuid();
                randomString = guid.ToString().Replace("-", "").Substring(0, 10);
                _genericAttributeService.SaveAttribute<string>(postProcessPaymentRequest.Order, EskadeniaHelper.ORDER_POSTFIX_STRING, randomString);
            }
            //get store location
            var storeLocation = _webHelper.GetStoreLocation();

            var parameters = new Dictionary<string, string>
            {
                // merchant code
                ["merchantCode"] = _eskadeniaPaymentSettings.MerchantCode,
                // channel code
                ["channelCode"] = _eskadeniaPaymentSettings.ChannelCode,
                // service type
                ["serviceType"] = serviceType,
                // transaction number // order id
                ["merchantTrxNo"] = postProcessPaymentRequest.Order.Id.ToString() + "-" + randomString,
                // order total
                ["payAmount"] = postProcessPaymentRequest.Order.OrderTotal.ToString(),
                // order currency
                ["currencyCode"] = "JOD" //postProcessPaymentRequest.Order.CustomerCurrencyCode.ToUpper() //TODO: hard coded currency code
            };

            if (serviceType == "proceedCheckout")
            {
                //customer email address and name
                string email = string.Empty;
                string customerName = string.Empty;

                if (order.ShippingAddress != null)
                {
                    email = order.ShippingAddress.Email;
                    customerName = $"{order.ShippingAddress.FirstName} {order.ShippingAddress.LastName}";
                }
                else if (order.BillingAddress != null)
                {
                    email = order.BillingAddress.Email;
                    customerName = $"{order.ShippingAddress.FirstName} {order.ShippingAddress.LastName}";
                }
                else
                {
                    email = order.Customer.Email;
                    customerName = _customerService.GetCustomerFullName(order.Customer);
                }
                parameters.Add("custEmail", email);
                parameters.Add("custNum", customerName);
                parameters.Add("PSP", _eskadeniaPaymentSettings.PSP);
                parameters.Add("callBackUrl", _webHelper.GetStoreLocation() + "PaymentEskadenia/PaymentResponse");
            }

            return parameters;
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            if (_workContext.WorkingCurrency.CurrencyCode.ToUpper() == "JOD" || _workContext.WorkingCurrency.CurrencyCode == "دينار")
                return false;
            
            return true;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _paymentService.CalculateAdditionalFee(cart, 0, true);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentEskadenia/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "PaymentEskadenia";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new EskadeniaPaymentSettings
            {
                MerchantCode = "COM-CC-05",
                ChannelCode = "303331",
                APIEndPoint = "http://10.1.11.162:2424/PaymentHub/",
                PSP = "2"
            });

            //Insert resource string
            InstallLocaleResources();

            // active widget at plugin installation time
            _widgetSettings.ActiveWidgetSystemNames.Add(this.PluginDescriptor.SystemName);
            _settingService.SaveSetting(_widgetSettings);

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<EskadeniaPaymentSettings>();

            //Delete resource string
            DeleteLocalResources();

            // remove widget at plugin un-installation time
            _widgetSettings.ActiveWidgetSystemNames.Remove(this.PluginDescriptor.SystemName);
            _settingService.SaveSetting(_widgetSettings);

            base.Uninstall();
        }


        #region widget

        public IList<string> GetWidgetZones()
        {
            return new List<string> { }; // TODO : Redirect to Original payment
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "";
        }

        #endregion
        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get { return RecurringPaymentType.NotSupported; }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Redirection; }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to Eskadenia site to complete the payment"

            get { return null; } //_localizationService.GetResource("Plugins.Payments.Eskadenia.PaymentMethodDescription")
        }

        #endregion
    }
}