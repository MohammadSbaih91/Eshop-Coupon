using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Http.Extensions;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.OrangeMoney.Factory;
using Nop.Plugin.Payments.OrangeMoney.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Checkout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Nop.Plugin.Payments.OrangeMoney.Controllers
{
    public class PaymentOrangeMoneyController : BasePaymentController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly IPermissionService _permissionService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly OrangeMoneyPaymentSettings _orangeMoneyPaymentSettings;
        private readonly IRepository<Language> _languagerepository;
        private readonly INopFileProvider _fileProvider;
        private readonly ICustomerService _customerService;
        private readonly ICurrencyService _currencyService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEncryptionService _encryptionService;
        private readonly AddressSettings _addressSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly IRepaymentFactory _repaymentFactory;
        private readonly OrderSettings _orderSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly CommonSettings _commonSettings;

        #endregion

        #region Ctor

        public PaymentOrangeMoneyController(IEncryptionService encryptionService,
            IWorkContext workContext,
            ISettingService settingService,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ICheckoutModelFactory checkoutModelFactory,
            IPermissionService permissionService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            ILogger logger,
            IWebHelper webHelper,
            ShoppingCartSettings shoppingCartSettings,
            OrangeMoneyPaymentSettings orangeMoneyPaymentSettings,
            IRepository<Language> languagerepository,
            INopFileProvider fileProvider,
            ICustomerService customerService,
            ICurrencyService currencyServicer,
            IHttpContextAccessor httpContextAccessor,
            AddressSettings addressSettings,
            PaymentSettings paymentSettings,
            IPluginFinder pluginFinder,
            IRepaymentFactory repaymentFactory,
            OrderSettings orderSettings,
            RewardPointsSettings rewardPointsSettings,
             CommonSettings commonSettings)
        {
            _encryptionService = encryptionService;
            this._workContext = workContext;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._checkoutModelFactory = checkoutModelFactory;
            this._permissionService = permissionService;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._storeContext = storeContext;
            this._logger = logger;
            this._webHelper = webHelper;
            this._shoppingCartSettings = shoppingCartSettings;
            this._orangeMoneyPaymentSettings = orangeMoneyPaymentSettings;
            this._languagerepository = languagerepository;
            this._fileProvider = fileProvider;
            this._customerService = customerService;
            this._currencyService = currencyServicer;
            this._httpContextAccessor = httpContextAccessor;
            this._addressSettings = addressSettings;
            this._paymentSettings = paymentSettings;
            this._pluginFinder = pluginFinder;
            this._repaymentFactory = repaymentFactory;
            this._orderSettings = orderSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._commonSettings = commonSettings;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var orangeMoneyPaymentSettings = _settingService.LoadSetting<OrangeMoneyPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                MerchantCode = orangeMoneyPaymentSettings.MerchantCode,
                ChannelCode = orangeMoneyPaymentSettings.ChannelCode,
                PSP = orangeMoneyPaymentSettings.PSP,
                ActiveStoreScopeConfiguration = storeScope,
                APIEndPoint = orangeMoneyPaymentSettings.APIEndPoint,
                CallBackURL = _webHelper.GetStoreLocation() + "PaymentOrangeMoney/PaymentResponse"
            };

            if (storeScope > 0)
            {
                model.MerchantCode_OverrideForStore = _settingService.SettingExists(orangeMoneyPaymentSettings, x => x.MerchantCode, storeScope);
                model.ChannelCode_OverrideForStore = _settingService.SettingExists(orangeMoneyPaymentSettings, x => x.ChannelCode, storeScope);
                model.PSP_OverrideForStore = _settingService.SettingExists(orangeMoneyPaymentSettings, x => x.PSP, storeScope);
            }

            return View("~/Plugins/Payments.OrangeMoney/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var OrangeMoneyPaymentSettings = _settingService.LoadSetting<OrangeMoneyPaymentSettings>(storeScope);

            //save settings
            OrangeMoneyPaymentSettings.MerchantCode = model.MerchantCode;
            OrangeMoneyPaymentSettings.ChannelCode = model.ChannelCode;
            OrangeMoneyPaymentSettings.PSP = model.PSP;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(OrangeMoneyPaymentSettings, x => x.MerchantCode, model.MerchantCode_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(OrangeMoneyPaymentSettings, x => x.ChannelCode, model.ChannelCode_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(OrangeMoneyPaymentSettings, x => x.PSP, model.PSP_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        public IActionResult PaymentResponse()
        {
            // success response - http://localhost:15536/PaymentOrangeMoney/PaymentResponse/?merchantTrxNo=9999123&patTrxStatus=OPC-00000&payTrxNo=OES-20190929114619173&payAmount=66.0&payTrxDate=29-09-2019&secureHash=$2a$10$qasKb28TEfmc1mQyPK2JhuYojdfyyYK78R5K8egyESDQCJvBgVBPa
            // fail response with wrong card - http://localhost:15536/PaymentOrangeMoney/PaymentResponse?merchantTrxNo=9999124&patTrxStatus=OPC-00116&payTrxNo=OES-20190929115909174&payAmount=66.0&payTrxDate=&secureHash=
            //https://google.com/merchantTrxNo=om900&patTrxStatus=OPC-00000&payTrxNo=OES-20210915115933490&payAmount=1.2&transactionReference=03515092021120020208053&payTrxDate=15-09-2021&secureHash=$2a$10$rzaPx74KTLdjvBjW0uHvBuzydLswQOXYmEzWkH5ERDacQr/Zcr6tW

            string merchantTrxNo = string.Empty;
            string patTrxStatus = string.Empty;
            string payTrxNo = string.Empty;
            decimal payAmount = decimal.Zero;
            string payTrxDate = string.Empty;
            string secureHash = string.Empty;
            string transactionReference = string.Empty;

            //merchantTrxNo=11878-5d13db08e8&payTrxStatus=OPC-00000&payTrxNo=OES-20210920141140469&payAmount=68.38&payTrxDate=&secureHash=
            merchantTrxNo = _webHelper.QueryString<string>("merchantTrxNo");
            patTrxStatus = _webHelper.QueryString<string>("patTrxStatus");
            payTrxNo = _webHelper.QueryString<string>("payTrxNo");
            payAmount = _webHelper.QueryString<decimal>("payAmount");
            payTrxDate = _webHelper.QueryString<string>("payTrxDate");
            secureHash = _webHelper.QueryString<string>("secureHash");
            try
            {
                transactionReference = _webHelper.QueryString<string>("transactionReference");
            }
            catch (Exception)
            {
                _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "transactionReference Is not getting from OM");
            }
            
            
            int orderId = 0;

            // get order id from merchant trx code
            var orderMerchantNo = merchantTrxNo.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (orderMerchantNo.Length > 0)
            {
                int.TryParse(orderMerchantNo[0], out orderId);
            }
            var order = _orderService.GetOrderById(orderId);

            // if order null then redirec to homepage
            if (order == null)
                return RedirectToAction("Index", "Home", new { area = "" });

            decimal orderTotal = Math.Round(order.OrderTotal,2);

            // 1. check patTrxStatus code
            // if OPC-00000 is them payment transaction happend
            // now need to call PayChyeck API for check payment status
            // if transaction done and success then mark order as paid
            // if pending then mark as pending
            // if code is not OPC-00000 then payment transaction not happened

            // 1. check patTrxStatus code
            string responseMessage = string.Empty;
            switch (patTrxStatus)
            {
                case Nop.Plugin.Payments.OrangeMoney.StatusCode.OPC00000:
                    #region Sucesss
                    try
                    {
                        // now need to call PayChyeck API for check payment status
                        string endPointURL = _orangeMoneyPaymentSettings.APIEndPoint;
                        string requestUrl = endPointURL + "rest/Communicator/PayCheck";
                        var httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUrl);

                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";
                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            string json = "{\"merchantCode\":\"" + _orangeMoneyPaymentSettings.MerchantCode + "\"," +
                                          "\"channelCode\":\"" + _orangeMoneyPaymentSettings.ChannelCode + "\"," +
                                          "\"merchantTrxNo\":\"" + merchantTrxNo + "\"}";

                            streamWriter.Write(json);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }

                        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var response = JsonConvert.DeserializeObject<PaymentResponseModel>(streamReader.ReadToEnd());

                            #region payment response

                            var sb = new StringBuilder();
                            sb.AppendLine("merchantTrxNo: " + response.merchantTrxNo);
                            sb.AppendLine("payTrxStatus: " + response.payTrxStatus);
                            sb.AppendLine("description: " + response.description);
                            sb.AppendLine("payTrxNo: " + response.payTrxNo);
                            sb.AppendLine("payAmount: " + response.payAmount);
                            sb.AppendLine("payTrxDate: " + payTrxDate);
                            sb.AppendLine("transactionReference: " + transactionReference);
                            
                            // update payment transaction detail
                            order.AuthorizationTransactionId = transactionReference;
                            order.AuthorizationTransactionResult = response.description;
                            order.AuthorizationTransactionCode = response.payTrxStatus;
                            _orderService.UpdateOrder(order);

                            #endregion

                            if (response.payTrxStatus == Nop.Plugin.Payments.OrangeMoney.StatusCode.OPC00000)
                            {
                                // payment success
                                string successMessage = "Payment Success. Response message -" + response.description + "-" + sb;
                                order.OrderNotes.Add(new OrderNote()
                                {
                                    Note = successMessage,
                                    DisplayToCustomer = false,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                                _orderService.UpdateOrder(order);

                                // mark order as paid
                                if (_orderProcessingService.CanMarkOrderAsPaid(order))
                                {
                                    _orderProcessingService.MarkOrderAsPaid(order);
                                }
                            }
                            else
                            {
                                // payment fail
                                string failMessage = "Payment fail. Response message -" + response.description + "-" + sb;
                                order.OrderNotes.Add(new OrderNote()
                                {
                                    Note = failMessage,
                                    DisplayToCustomer = false,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                                _orderService.UpdateOrder(order);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error on success payment for order id -" + order.Id + ". " + ex.Message, ex);
                    }

                    #endregion

                    break;
                case Nop.Plugin.Payments.OrangeMoney.StatusCode.OPC00130:
                    #region Merchant code wrong

                    //not valid
                    responseMessage = "Merchant Code Does Not Exist. Response from Payment Gateway is - " + _httpContextAccessor.HttpContext.Request.QueryString.ToString();

                    //order note
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = responseMessage,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    #endregion
                    break;
                case Nop.Plugin.Payments.OrangeMoney.StatusCode.OPC00131:
                    #region Currency Does Not Exist

                    //not valid
                    responseMessage = "Currency(" + order.CustomerCurrencyCode + ") Does Not Exist. Response from Payment Gateway is - " + _httpContextAccessor.HttpContext.Request.QueryString.ToString();

                    //order note
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = responseMessage,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    #endregion
                    break;
                case Nop.Plugin.Payments.OrangeMoney.StatusCode.OPC00132:
                    #region Transaction Not Found
                    //not valid
                    responseMessage = "Transaction Not Found. Response from Payment Gateway is - " + _httpContextAccessor.HttpContext.Request.QueryString.ToString();

                    //order note
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = responseMessage,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    #endregion
                    break;
                default:
                    #region Fail

                    //not valid
                    var responsMessage = "Response from Payment Gateway is - " + _httpContextAccessor.HttpContext.Request.QueryString.ToString();

                    //order note
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = responsMessage,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                    #endregion
                    break;
            }

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }

        #endregion

        #region Payment Method

        public List<ShoppingCartItem> PrepareCartFromOrderDetail(Order order)
        {

            List<ShoppingCartItem> cart = new List<ShoppingCartItem>();
            var model = new CheckoutPaymentMethodModel();

            foreach (var orderItem in order.OrderItems)
            {
                var sci = new ShoppingCartItem()
                {
                    AttributesXml = orderItem.AttributesXml,
                    CreatedOnUtc = order.CreatedOnUtc,
                    UpdatedOnUtc = order.CreatedOnUtc,
                    CustomerId = order.CustomerId,
                    Customer = order.Customer,
                    CustomerEnteredPrice = 0,
                    ProductId = orderItem.ProductId,
                    Product = orderItem.Product,
                    Quantity = orderItem.Quantity,
                    RentalEndDateUtc = orderItem.RentalEndDateUtc,
                    RentalStartDateUtc = orderItem.RentalStartDateUtc,
                    ShoppingCartTypeId = (Int32)ShoppingCartType.ShoppingCart,
                    ShoppingCartType = ShoppingCartType.ShoppingCart,
                    StoreId = order.StoreId
                };

                cart.Add(sci);
            }

            return cart;
        }

        protected virtual ProcessPaymentResult GetProcessPaymentResult(ProcessPaymentRequest processPaymentRequest)
        {
            ProcessPaymentResult processPaymentResult;

            var paymentMethod =
                _paymentService.LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);

            //standard cart
            processPaymentResult = _paymentService.ProcessPayment(processPaymentRequest);

            return processPaymentResult;
        }

        public IActionResult PaymentMethod(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);

            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return Challenge();

            HttpContext.Session.Set<int>("RePaymentOrdreId", orderId);

            // prepare cart for get values
            List<ShoppingCartItem> cart = PrepareCartFromOrderDetail(order);

            var model = new CheckoutPaymentMethodModel();

            //filter by country
            var filterByCountryId = 0;
            if (_addressSettings.CountryEnabled &&
                _workContext.CurrentCustomer.BillingAddress != null &&
                _workContext.CurrentCustomer.BillingAddress.Country != null)
            {
                filterByCountryId = _workContext.CurrentCustomer.BillingAddress.Country.Id;
            }

            //payment is required
            model = _repaymentFactory.PreparePaymentMethodModel(cart, filterByCountryId);

            if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                model.PaymentMethods.Count == 1 && !model.DisplayRewardPoints)
            {
                //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
                //so customer doesn't have to choose a payment method

                var selectedPaymentMethodSystemName = model.PaymentMethods[0].PaymentMethodSystemName;
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute,
                    selectedPaymentMethodSystemName, _storeContext.CurrentStore.Id);

                var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(selectedPaymentMethodSystemName);
                if (paymentMethodInst == null ||
                    !_paymentService.IsPaymentMethodActive(paymentMethodInst) ||
                    !_pluginFinder.AuthenticateStore(paymentMethodInst.PluginDescriptor, _storeContext.CurrentStore.Id) ||
                    !_pluginFinder.AuthorizedForUser(paymentMethodInst.PluginDescriptor, _workContext.CurrentCustomer))
                    throw new Exception("Selected payment method can't be parsed");
            }

            return View("~/Plugins/Payments.OrangeMoney/Views/PaymentOrangeMoney/PaymentMethod.cshtml", model);
        }

        public virtual IActionResult OpcSavePaymentMethod(string paymentmethod, CheckoutPaymentMethodModel model)
        {
            try
            {
                int orderId = HttpContext.Session.Get<int>("RePaymentOrdreId");
                var order = _orderService.GetOrderById(orderId);

                if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                    return Challenge();

                // prepare cart for get values
                List<ShoppingCartItem> cart = PrepareCartFromOrderDetail(order);

                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(_localizationService.GetResource("Checkout.Disabled"));

                //payment method 
                if (string.IsNullOrEmpty(paymentmethod))
                    throw new Exception("Selected payment method can't be parsed");

                //reward points
                if (_rewardPointsSettings.Enabled)
                {
                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute, model.UseRewardPoints,
                        _storeContext.CurrentStore.Id);
                }

                ////Check whether payment workflow is required
                //var isPaymentWorkflowRequired = _orderProcessingService.IsPaymentWorkflowRequired(cart);
                //if (!isPaymentWorkflowRequired)
                //{
                //    //payment is not required
                //    _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                //        NopCustomerDefaults.SelectedPaymentMethodAttribute, null, _storeContext.CurrentStore.Id);

                //    var confirmOrderModel = new CheckoutConfirmModel
                //    {
                //        //terms of service
                //        TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage,
                //        TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks
                //    };

                //    return Json(new
                //    {
                //        update_section = new UpdateSectionJsonModel
                //        {
                //            name = "confirm-order",
                //            html = RenderPartialViewToString("~/Plugins/Payments.OrangeMoney/Views/PaymentOrangeMoney/OpcConfirmOrder.cshtml", confirmOrderModel)
                //        },
                //        goto_section = "confirm_order"
                //    });
                //}

                var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(paymentmethod);
                if (paymentMethodInst == null ||
                    !_paymentService.IsPaymentMethodActive(paymentMethodInst) ||
                    !_pluginFinder.AuthenticateStore(paymentMethodInst.PluginDescriptor, _storeContext.CurrentStore.Id) ||
                    !_pluginFinder.AuthorizedForUser(paymentMethodInst.PluginDescriptor, _workContext.CurrentCustomer))
                    throw new Exception("Selected payment method can't be parsed");

                //save
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, paymentmethod, _storeContext.CurrentStore.Id);

                return OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        protected virtual JsonResult OpcLoadStepAfterPaymentMethod(IPaymentMethod paymentMethod, List<ShoppingCartItem> cart)
        {
            //if (paymentMethod.SkipPaymentInfo ||
            //    (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection && _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
            //{
            //    //skip payment info page
            //    var paymentInfo = new ProcessPaymentRequest();

            //    //session save
            //    HttpContext.Session.Set("OrderPaymentInfo", paymentInfo);

            //    var confirmOrderModel = _checkoutModelFactory.PrepareConfirmOrderModel(cart);
            //    return Json(new
            //    {
            //        update_section = new UpdateSectionJsonModel
            //        {
            //            name = "confirm-order",
            //            html = RenderPartialViewToString("~/Plugins/Payments.OrangeMoney/Views/PaymentOrangeMoney/OpcConfirmOrder.cshtml", confirmOrderModel)
            //        },
            //        goto_section = "confirm_order"
            //    });
            //}

            //return payment info page
            var paymenInfoModel = _checkoutModelFactory.PreparePaymentInfoModel(paymentMethod);
            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "payment-info",
                    html = RenderPartialViewToString("~/Plugins/Payments.OrangeMoney/Views/PaymentOrangeMoney/OpcPaymentInfo.cshtml", paymenInfoModel)
                },
                goto_section = "payment_info"
            });
        }

        public virtual IActionResult OpcSavePaymentInfo(IFormCollection form)
        {
            try
            {
                int orderId = HttpContext.Session.Get<int>("RePaymentOrdreId");
                var order = _orderService.GetOrderById(orderId);

                if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                    return Challenge();

                // prepare cart for get values
                List<ShoppingCartItem> cart = PrepareCartFromOrderDetail(order);

                var paymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);
                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);
                if (paymentMethod == null)
                    throw new Exception("Payment method is not selected");

                var warnings = paymentMethod.ValidatePaymentForm(form);
                foreach (var warning in warnings)
                    ModelState.AddModelError("", warning);
                if (ModelState.IsValid)
                {
                    //get payment info
                    var paymentInfo = paymentMethod.GetPaymentInfo(form);

                    //session save
                    HttpContext.Session.Set("OrderPaymentInfo", paymentInfo);

                    //Updating order payment method and payment 
                    //place order
                    var processPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");

                    if (processPaymentRequest == null)
                    {
                        //Check whether payment workflow is required
                        if (_orderProcessingService.IsPaymentWorkflowRequired(cart))
                        {
                            throw new Exception("Payment information is not entered");
                        }

                        processPaymentRequest = new ProcessPaymentRequest();
                    }

                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = order
                    };

                    order.PaymentMethodSystemName = paymentMethodSystemName;

                    processPaymentRequest.OrderTotal = Math.Round(order.OrderTotal);
                    processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                    processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
                    processPaymentRequest.PaymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);

                    var processPaymentResult = GetProcessPaymentResult(processPaymentRequest);
                    if (order.PaymentMethodSystemName == "Payments.Manual")
                    {
                        order.AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber;
                        order.CardType = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardType) : string.Empty;
                        order.CardName = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardName) : string.Empty;
                        order.CardNumber = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardNumber) : string.Empty;
                        order.MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(processPaymentRequest.CreditCardNumber));
                        order.CardCvv2 = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardCvv2) : string.Empty;
                        order.CardExpirationMonth = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireMonth.ToString()) : string.Empty;
                        order.CardExpirationYear = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireYear.ToString()) : string.Empty;
                        order.PaymentMethodSystemName = processPaymentRequest.PaymentMethodSystemName;
                    }

                    _orderService.UpdateOrder(order);

                    if (paymentMethod == null)
                        //success
                        return Json(new { success = 1 });

                    if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                    {
                        //Redirection will not work because it's AJAX request.
                        //That's why we don't process it here (we redirect a user to another page where he'll be redirected)
                        //redirect
                        return Json(new
                        {
                            redirect = $"{_webHelper.GetStoreLocation()}PaymentOrangeMoney/OpcCompleteRedirectionPayment"
                        });
                    }

                    _paymentService.PostProcessPayment(postProcessPaymentRequest);

                    //clear session
                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                    HttpContext.Session.Remove("RePaymentOrdreId");

                    //success
                    return Json(new { redirect = $"{_webHelper.GetStoreLocation()}checkout/completed/" + order.Id });
                }

                //If we got this far, something failed, redisplay form
                var paymenInfoModel = _checkoutModelFactory.PreparePaymentInfoModel(paymentMethod);
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "payment-info",
                        html = RenderPartialViewToString("~/Plugins/Payments.OrangeMoney/Views/PaymentOrangeMoney/OpcPaymentInfo.cshtml", paymenInfoModel)
                    }
                });
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public virtual IActionResult OpcCompleteRedirectionPayment()
        {
            try
            {
                int orderId = HttpContext.Session.Get<int>("RePaymentOrdreId");
                var order = _orderService.GetOrderById(orderId);

                //get the order
                if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                    return Challenge();

                // prepare cart for get values
                List<ShoppingCartItem> cart = PrepareCartFromOrderDetail(order);

                //validation
                if (!_orderSettings.OnePageCheckoutEnabled)
                    return RedirectToRoute("HomePage");

                if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                    return Challenge();

                var processPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");

                var paymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);
                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(paymentMethodSystemName);

                if (paymentMethod == null)
                    return RedirectToRoute("HomePage");
                if (paymentMethod.PaymentMethodType != PaymentMethodType.Redirection)
                    return RedirectToRoute("HomePage");

                //Redirection will not work on one page checkout page because it's AJAX request.
                //That's why we process it here
                var postProcessPaymentRequest = new PostProcessPaymentRequest
                {
                    Order = order
                };

                _paymentService.PostProcessPayment(postProcessPaymentRequest);

                if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                {
                    //redirection or POST has been done in PostProcessPayment
                    return Content("Redirected");
                }

                //if no redirection has been done (to a third-party payment page)
                //theoretically it's not possible
                HttpContext.Session.Remove("RePaymentOrdreId");
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Content(exc.Message);
            }
        }

        #endregion
    }
}