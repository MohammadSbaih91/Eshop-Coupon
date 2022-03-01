using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Internal;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Http.Extensions;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Web.Extensions;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Security;
using Nop.Web.Models.Checkout;

namespace Nop.Web.Controllers
{

    [HttpsRequirement(SslRequirement.Yes)]
    public partial class CheckoutController
    {
        #region Fields
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly ICheckoutAttributeService _checkoutAttributeService;


        private readonly IDownloadService _downloadService;

        private readonly INopFileProvider _fileProvider;
        private readonly IHostingEnvironment _hostingEnvironment;
        #endregion

        #region Ctor
        public CheckoutController(AddressSettings addressSettings,
            CustomerSettings customerSettings,
            IDownloadService downloadService,
            INopFileProvider fileProvider,
            IHostingEnvironment hostingEnvironment,
             ICheckoutAttributeService checkoutAttributeService,
            IAddressAttributeParser addressAttributeParser,
            IAddressService addressService,
            ICheckoutModelFactory checkoutModelFactory,
            ICountryService countryService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPluginFinder pluginFinder,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IWorkContext workContext,
            OrderSettings orderSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings, INewsLetterSubscriptionService newsLetterSubscriptionService,
            IAddressModelFactory addressModelFactory)
        {
            this._addressSettings = addressSettings;
            this._checkoutAttributeService = checkoutAttributeService;
            this._customerSettings = customerSettings;
            this._addressAttributeParser = addressAttributeParser;
            this._addressService = addressService;
            this._checkoutModelFactory = checkoutModelFactory;
            this._countryService = countryService;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._logger = logger;
            this._orderProcessingService = orderProcessingService;
            this._orderService = orderService;
            this._paymentService = paymentService;
            this._pluginFinder = pluginFinder;
            this._shippingService = shippingService;
            this._shoppingCartService = shoppingCartService;
            this._stateProvinceService = stateProvinceService;
            this._storeContext = storeContext;
            this._webHelper = webHelper;
            this._workContext = workContext;
            this._orderSettings = orderSettings;
            this._paymentSettings = paymentSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._shippingSettings = shippingSettings;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._addressModelFactory = addressModelFactory;
            this._downloadService = downloadService;
            this._fileProvider = fileProvider;
            this._hostingEnvironment = hostingEnvironment;
        }

        #endregion

        #region Utilities
        protected virtual JsonResult OpcLoadStepAfterShippingMethod(List<ShoppingCartItem> cart, bool isPickUpInStore = false)
        {
            //Check whether payment workflow is required
            //we ignore reward points during cart total calculation
            var isPaymentWorkflowRequired = _orderProcessingService.IsPaymentWorkflowRequired(cart, false);
            if (isPaymentWorkflowRequired)
            {
                //filter by country
                var filterByCountryId = 0;
                if (_addressSettings.CountryEnabled &&
                    _workContext.CurrentCustomer.BillingAddress != null &&
                    _workContext.CurrentCustomer.BillingAddress.Country != null)
                {
                    filterByCountryId = _workContext.CurrentCustomer.BillingAddress.Country.Id;
                }

                //payment is required
                var paymentMethodModel = _checkoutModelFactory.PreparePaymentMethodModel(cart, filterByCountryId);
                
                if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                    paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
                {
                    //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
                    //so customer doesn't have to choose a payment method

                    var selectedPaymentMethodSystemName = paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName;
                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute,
                        selectedPaymentMethodSystemName, _storeContext.CurrentStore.Id);

                    var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(selectedPaymentMethodSystemName);
                    if (paymentMethodInst == null ||
                        !_paymentService.IsPaymentMethodActive(paymentMethodInst) ||
                        !_pluginFinder.AuthenticateStore(paymentMethodInst.PluginDescriptor, _storeContext.CurrentStore.Id) ||
                        !_pluginFinder.AuthorizedForUser(paymentMethodInst.PluginDescriptor, _workContext.CurrentCustomer))
                        throw new Exception("Selected payment method can't be parsed");

                    return OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);
                }

                if (isPickUpInStore)
                {
                    //if we select pickup from store must be select Payments.CheckMoneyOrder
                    //so customer doesn't have to choose a payment method

                    var selectedPaymentMethodSystemName = "Payments.CheckMoneyOrder";
                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute,
                        selectedPaymentMethodSystemName, _storeContext.CurrentStore.Id);

                    var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(selectedPaymentMethodSystemName);
                    if (paymentMethodInst == null ||
                        !_paymentService.IsPaymentMethodActive(paymentMethodInst) ||
                        !_pluginFinder.AuthenticateStore(paymentMethodInst.PluginDescriptor, _storeContext.CurrentStore.Id) ||
                        !_pluginFinder.AuthorizedForUser(paymentMethodInst.PluginDescriptor, _workContext.CurrentCustomer))
                        throw new Exception("Selected payment method can't be parsed");

                    return OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);
                }

                //customer have to choose a payment method
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "payment-method",
                        html = RenderPartialViewToString("OpcPaymentMethods", paymentMethodModel)
                    },
                    goto_section = "payment_method"
                });
            }

            //payment is not required
            _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, null, _storeContext.CurrentStore.Id);

            var confirmOrderModel = _checkoutModelFactory.PrepareConfirmOrderModel(cart);
            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "confirm-order",
                    html = RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                },
                goto_section = "confirm_order"
            });
        }

        #endregion

        #region Methods (common)
        public virtual IActionResult Completed(int? orderId)
        {
            //validation
            if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            Order order = null;
            if (orderId.HasValue)
            {
                //load order by identifier (if provided)
                order = _orderService.GetOrderById(orderId.Value);
            }

            if (order != null)
            {
                if (order.PickUpInStore)
                {
                    return RedirectToRoute("appointmentbooking.Appointment", new { orderid = order.Id, isChangeAppointent = false });
                }
            }

            if (order == null)
            {
                order = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1)
                    .FirstOrDefault();
            }
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
            {
                return RedirectToRoute("HomePage");
            }

            //disable "order completed" page?
            if (_orderSettings.DisableOrderCompletedPage)
            {
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });
            }

            //model
            var model = _checkoutModelFactory.PrepareCheckoutCompletedModel(order);
            return View(model);
        }
        #endregion


        [HttpPost]
        public virtual IActionResult UploadIDFile()
        {
            //var attribute = _checkoutAttributeService.GetCheckoutAttributeById(attributeId);
            //if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
            //{
            //    return Json(new
            //    {
            //        success = false,
            //        downloadGuid = Guid.Empty
            //    });
            //}



            var id = Guid.NewGuid();
            //todo add foreche
            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No file uploaded",
                    downloadGuid = Guid.Empty
                });
            }
            var fileBinary = _downloadService.GetDownloadBits(httpPostedFile);

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);
            var contentType = httpPostedFile.ContentType;
            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();
            //compare in bytes
            var maxFileSizeBytes = 5000 * 1024;
            if (fileBinary.Length > maxFileSizeBytes)
            {
                //when returning JSON the mime-type must be set to text/plain
                //otherwise some browsers will pop-up a "Save As" dialog.
                return Json(new
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"),
                        maxFileSizeBytes),
                    downloadGuid = Guid.Empty
                });
            }
            //  check images extensions
            string[] extensionsArray = { ".jpg", ".png", ".gif", ".jpeg", ".pdf" };

            if (extensionsArray.Contains(fileExtension) == false)
            {
                return Json(new
                {
                    success = false,
                    message = string.Format(
                       _localizationService.GetResource("ShoppingCart.FailExtensions"),
                       maxFileSizeBytes),
                    downloadGuid = Guid.Empty
                });
            }
            //System.IO.File.WriteAllBytes(_hostingEnvironment.WebRootPath + "\\Orders\\" + id + fileExtension, fileBinary);
            var directory = "images\\Orders\\";
            var filePath = _fileProvider.Combine(EShopHelper.GetAbsolutePath(directory), id + fileExtension);

            using (var fileStream = new FileStream(filePath, FileMode.CreateNew))
            {
                httpPostedFile.CopyTo(fileStream);
            }
            var nopConfig = EngineContext.Current.Resolve<NopConfig>();
            var download = new Download
            {
                DownloadGuid = id,
                UseDownloadUrl = false,
                DownloadUrl = filePath.Replace(nopConfig.SharedFileStorageContainerName+"\\images\\"
                ,"app-images\\"),
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            //_downloadService.InsertDownload(download);
            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                downloadUrl = download.DownloadUrl,
                downloadGuid = id + fileExtension
            });
        }


        public virtual IActionResult OpcSaveBilling(CheckoutBillingAddressModel model)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(_localizationService.GetResource("Checkout.Disabled"));

                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                int.TryParse(model.Form["billing_address_id"], out int billingAddressId);

                if (billingAddressId > 0)
                {
                    //existing address
                    var address = _workContext.CurrentCustomer.Addresses?.FirstOrDefault(a => a.Id == billingAddressId);
                    if (address == null)
                        throw new Exception("Address can't be loaded");

                    _workContext.CurrentCustomer.BillingAddress = address;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }
                else
                {
                    //new address
                    var newAddress = model.BillingNewAddress;

                    //custom address attributes
                    var customAttributes = _addressAttributeParser.ParseCustomAddressAttributes(model.Form);
                    var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    ModelState.Remove("BillingNewAddress.City");
                    ModelState.Remove("BillingNewAddress.Address1");
                    ModelState.Remove("BillingNewAddress.Address2");
                    ModelState.Remove("BillingNewAddress.CountryName");
                    ModelState.Remove("BillingNewAddress.CountryId");
                    ModelState.Remove("BillingNewAddress.StudentID");
                    ModelState.Remove("BillingNewAddress.uploadStudentID");
                    string uploadID = HttpContext.Request.Form["uploadID"].ToString();

                    //validate model
                   
                    if (model.BillingNewAddress.NationalityType == NationalityType.NationalId 
                        && model.BillingNewAddress.IdentityCardOrPassport.Length != 10)
                    {
                        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                        ModelState.AddModelError("BillingNewAddress.IdentityCardOrPassport"
                                    , localizationService.GetResource("Address.Fields.IdentityCardOrPassport.MaxLength"));
                    }
                    if (uploadID == "")
                    {
                        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                     

                            ModelState.AddModelError("BillingNewAddress.uploadID"
                                   , localizationService.GetResource("Address.Fields.UploadID.Required"));
                        
                    }
                   
                    if (cart.Any(x=>x.Product.IsStudentIdNeeded))
                    {
                       
                       

                        if (!ModelState.IsValid || HttpContext.Request.Form["uploadStudentID"].ToString() == ""
                            || HttpContext.Request.Form["BillingNewAddress.StudentID"].ToString() == "")
                        {
                            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

                            if (HttpContext.Request.Form["uploadStudentID"].ToString() == "")
                            {
                                ModelState.AddModelError("BillingNewAddress.uploadStudentID"
                                    , localizationService.GetResource("Address.Fields.UploadStudentID.Required"));
                            }
                            if (HttpContext.Request.Form["BillingNewAddress.StudentID"].ToString() == "")
                            {
                                ModelState.AddModelError("BillingNewAddress.StudentID"
                                    , localizationService.GetResource("Address.Fields.StudentID.Required"));
                            }
                            //model is not valid. redisplay the form with errors
                            var billingAddressModel = _checkoutModelFactory.PrepareBillingAddressModel(cart,
                                selectedCountryId: newAddress.CountryId,
                                overrideAttributesXml: customAttributes);
                            billingAddressModel.NewAddressPreselected = true;
                            billingAddressModel.BillingNewAddress.StudentID
                                = newAddress.StudentID;
                            billingAddressModel.BillingNewAddress.UploadID 
                                = HttpContext.Request.Form["UploadID"].ToString()==""?null
                                : HttpContext.Request.Form["UploadID"].ToString();
                            return Json(new
                            {
                                update_section = new UpdateSectionJsonModel
                                {
                                    name = "billing",
                                    html = RenderPartialViewToString("OpcBillingAddress", billingAddressModel)
                                },
                                wrong_billing_address = true,
                            });
                        }
                    }
                    else if(!ModelState.IsValid )
                    {
                                            

                        //model is not valid. redisplay the form with errors
                        var billingAddressModel = _checkoutModelFactory.PrepareBillingAddressModel(cart,
                            selectedCountryId: newAddress.CountryId,
                            overrideAttributesXml: customAttributes);
                        billingAddressModel.NewAddressPreselected = true;
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel
                            {
                                name = "billing",
                                html = RenderPartialViewToString("OpcBillingAddress", billingAddressModel)
                            },
                            wrong_billing_address = true,
                        });
                    }

                    


                    //try to find an address with the same values (don't duplicate records)
                    var address = _addressService.FindAddress(_workContext.CurrentCustomer.Addresses.ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes,
                        newAddress.Civility, newAddress.Nationality,
                        newAddress.NationalityType, newAddress.IdentityCardOrPassport);
                    if (address == null)
                    {
                        //address is not found. let's create a new one
                        address = newAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;
                        address.StudentID = HttpContext.Request.Form["BillingNewAddress.StudentID"];
                       // address.UploadID = HttpContext.Request.Form["uploadID"];
                        address.UploadStudentID = HttpContext.Request.Form["uploadStudentID"];
                        address.UploadID = uploadID;
                        //some validation
                        if (address.CountryId == 0)
                            address.CountryId = null;
                        if (address.StateProvinceId == 0)
                            address.StateProvinceId = null;
                        if (address.CountryId.HasValue && address.CountryId.Value > 0)
                        {
                            address.Country = _countryService.GetCountryById(address.CountryId.Value);
                        }
                        //_workContext.CurrentCustomer.Addresses.Add(address);
                        _workContext.CurrentCustomer.CustomerAddressMappings.Add(new CustomerAddressMapping { Address = address });
                    }
                    _workContext.CurrentCustomer.BillingAddress = address;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }

                SubscribeNewsletter(model.BillingNewAddress);

                if (!model.PickUpInStore && _shoppingCartService.ShoppingCartRequiresShipping(cart))
                {
                    ////shipping is required
                    //if (_shippingSettings.ShipToSameAddress && model.ShipToSameAddress && _workContext.CurrentCustomer.BillingAddress.Country != null && _workContext.CurrentCustomer.BillingAddress.Country.AllowsShipping)
                    //{
                    //    //ship to the same address
                    //    _workContext.CurrentCustomer.ShippingAddress = _workContext.CurrentCustomer.BillingAddress;
                    //    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                    //    //reset selected shipping method (in case if "pick up in store" was selected)
                    //    _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, _storeContext.CurrentStore.Id);
                    //    _genericAttributeService.SaveAttribute<PickupPoint>(_workContext.CurrentCustomer, NopCustomerDefaults.SelectedPickupPointAttribute, null, _storeContext.CurrentStore.Id);
                    //    //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 
                    //    return OpcLoadStepAfterShippingAddress(cart);
                    //}

                    //do not ship to the same address
                    var shippingAddressModel = _checkoutModelFactory.PrepareShippingAddressModel(prePopulateNewAddressWithCustomerFields: true);
                    shippingAddressModel.PickUpInStore = model.PickUpInStore;
                    ModelState.Clear();
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "shipping",
                            html = RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                        },
                        goto_section = "shipping"
                    });
                }

                //shipping is not required
                _workContext.CurrentCustomer.ShippingAddress = null;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);

                //load next step
                var json = OpcLoadStepAfterShippingMethod(cart, model.PickUpInStore);
                if (model.PickUpInStore)
                    return OpcConfirmOrder();

                return json;
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public virtual IActionResult OpcSaveShipping(CheckoutShippingAddressModel model)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(_localizationService.GetResource("Checkout.Disabled"));

                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                if (!_shoppingCartService.ShoppingCartRequiresShipping(cart))
                    throw new Exception("Shipping is not required");

                //Update Billing Addres
                    ModelState.Clear();

                if (model.BillingNewAddress != null)
                {
                    if (string.IsNullOrEmpty(model.BillingNewAddress.Address1))
                        ModelState.AddModelError("BillingNewAddress.Address1", _localizationService.GetResource("Account.Fields.StreetAddress.Required"));

                    if (string.IsNullOrEmpty(model.BillingNewAddress.Address2))
                        ModelState.AddModelError("BillingNewAddress.Address2", _localizationService.GetResource("Account.Fields.StreetAddress2.Required"));

                    if (string.IsNullOrEmpty(model.BillingNewAddress.City))
                        ModelState.AddModelError("BillingNewAddress.City", _localizationService.GetResource("Account.Fields.City.Required"));

                    if (model.BillingNewAddress.CountryId <= 0)
                        ModelState.AddModelError("BillingNewAddress.CountryId", _localizationService.GetResource("Address.Fields.Country.Required"));
                }

                //custom address attributes
                var customAttributes = _addressAttributeParser.ParseCustomAddressAttributes(model.Form);
                var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);

                if (!ModelState.IsValid)
                {
                    //model is not valid. redisplay the form with errors
                    var shippingAddressModel = _checkoutModelFactory.PrepareShippingAddressModel(
                        selectedCountryId: model.BillingNewAddress.CountryId,
                        overrideAttributesXml: customAttributes);
                    shippingAddressModel.NewAddressPreselected = true;
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "shipping",
                            html = RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                        }
                    });
                }

                _workContext.CurrentCustomer.BillingAddress.Address1 = model.BillingNewAddress.Address1;
                _workContext.CurrentCustomer.BillingAddress.Address2 = model.BillingNewAddress.Address2;
                _workContext.CurrentCustomer.BillingAddress.BuildingNo = model.BillingNewAddress.BuildingNo;
                _workContext.CurrentCustomer.BillingAddress.City = model.BillingNewAddress.City;
                _workContext.CurrentCustomer.BillingAddress.CountryId = model.BillingNewAddress.CountryId;
                _workContext.CurrentCustomer.BillingAddress.Country = _countryService.GetCountryById((int)model.BillingNewAddress.CountryId);
                model.BillingNewAddress.StudentID = _workContext.CurrentCustomer.BillingAddress.StudentID;
                model.BillingNewAddress.UploadID = _workContext.CurrentCustomer.BillingAddress.UploadID;
                model.BillingNewAddress.UploadStudentID = _workContext.CurrentCustomer.BillingAddress.UploadStudentID;
                model.BillingNewAddress.IsStudentIdNeeded = model.BillingNewAddress.StudentID == null ? false : true;


                _customerService.UpdateCustomer(_workContext.CurrentCustomer);

                //pickup point
                if (_shippingSettings.AllowPickUpInStore)
                {
                    if (model.PickUpInStore)
                    {
                        //no shipping address selected
                        _workContext.CurrentCustomer.ShippingAddress = null;
                        _customerService.UpdateCustomer(_workContext.CurrentCustomer);

                        var pickupPoint = model.Form["pickup-points-id"].ToString().Split(new[] { "___" }, StringSplitOptions.None);
                        var pickupPoints = _shippingService.GetPickupPoints(_workContext.CurrentCustomer.BillingAddress,
                            _workContext.CurrentCustomer, pickupPoint[1], _storeContext.CurrentStore.Id).PickupPoints.ToList();
                        var selectedPoint = pickupPoints.FirstOrDefault(x => x.Id.Equals(pickupPoint[0]));

                        if (selectedPoint == null)
                            throw new Exception("Pickup point is not allowed");

                        var pickUpInStoreShippingOption = new ShippingOption
                        {
                            Name = string.Format(_localizationService.GetResource("Checkout.PickupPoints.Name"), selectedPoint.Name),
                            Rate = selectedPoint.PickupFee,
                            Description = selectedPoint.Description,
                            ShippingRateComputationMethodSystemName = selectedPoint.ProviderSystemName
                        };
                        _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, pickUpInStoreShippingOption, _storeContext.CurrentStore.Id);
                        _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, NopCustomerDefaults.SelectedPickupPointAttribute, selectedPoint, _storeContext.CurrentStore.Id);

                        //load next step
                        // Comment this because if pickupinstore default payment method COD and order will placed
                        //return OpcLoadStepAfterShippingMethod(cart, true);

                        var json = OpcLoadStepAfterShippingMethod(cart, true);
                        return OpcConfirmOrder();
                        //UpdateSectionJsonModel updateSectionJsonModel = JsonConvert.DeserializeObject(json);
                    }

                    //set value indicating that "pick up in store" option has not been chosen
                    _genericAttributeService.SaveAttribute<PickupPoint>(_workContext.CurrentCustomer, NopCustomerDefaults.SelectedPickupPointAttribute, null, _storeContext.CurrentStore.Id);
                }

                int.TryParse(model.Form["shipping_address_id"], out int shippingAddressId);

                if (shippingAddressId > 0)
                {
                    //existing address
                    var address = _workContext.CurrentCustomer.Addresses.FirstOrDefault(a => a.Id == shippingAddressId);
                    if (address == null)
                        throw new Exception("Address can't be loaded");

                    _workContext.CurrentCustomer.ShippingAddress = address;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                }
                else
                {
                    //Fill default value from billing addres to shipping address
                    //new address
                    string address1 = "";
                    string address2 = "";
                    string city = "";

                    if (model.ShipToSameAddress)
                    {
                        address1 = new string(model.BillingNewAddress.Address1 ?? "");
                        address2 = new string(model.BillingNewAddress.Address2 ?? "");
                        city = new string(model.BillingNewAddress.City ?? "");
                    }
                    else
                    {
                        address1 = new string(model.ShippingNewAddress.Address1 ?? "");
                        address2 = new string(model.ShippingNewAddress.Address2 ?? "");
                        city = new string(model.ShippingNewAddress.City ?? "");
                    }

                    _addressModelFactory.PrepareAddressModel(model.ShippingNewAddress,
                    address: _workContext.CurrentCustomer.BillingAddress,
                    excludeProperties: false,
                    addressSettings: _addressSettings);

                    var lastAddress = _workContext.CurrentCustomer.BillingAddress;

                    var newAddress = model.ShippingNewAddress;
                    newAddress.Address1 = address1;
                    newAddress.Address2 = address2;
                    newAddress.City = city;
                    newAddress.CountryId = model.ShippingNewAddress.CountryId ?? 0;
                    newAddress.StudentID = lastAddress.StudentID;
                    newAddress.UploadID = lastAddress.UploadID;
                    newAddress.UploadStudentID = lastAddress.UploadStudentID;
                    newAddress.IsStudentIdNeeded = lastAddress.StudentID == null ? false : true;

                    ModelState.Clear();

                    
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }
                    if (!model.ShipToSameAddress)
                    {
                        //model.ShippingNewAddress.CountryId = 0;
                        if (model.ShippingNewAddress != null)
                        {
                            if (string.IsNullOrEmpty(model.ShippingNewAddress.Address1))
                                ModelState.AddModelError("ShippingNewAddress.Address1", _localizationService.GetResource("Account.Fields.StreetAddress.Required"));

                            if (string.IsNullOrEmpty(model.ShippingNewAddress.City))
                                ModelState.AddModelError("ShippingNewAddress.City", _localizationService.GetResource("Account.Fields.City.Required"));

                            if (model.ShippingNewAddress.CountryId <= 0)
                                ModelState.AddModelError("ShippingNewAddress.CountryId", _localizationService.GetResource("Address.Fields.Country.Required"));
                        }
                    }

                    //validate model
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var shippingAddressModel = _checkoutModelFactory.PrepareShippingAddressModel(
                            selectedCountryId: newAddress.CountryId,
                            overrideAttributesXml: customAttributes);
                        shippingAddressModel.NewAddressPreselected = true;
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel
                            {
                                name = "shipping",
                                html = RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                            }
                        });
                    }

                    ////Update Billing Addres
                    //if (model.BillingNewAddress != null)
                    //{
                    //    if (string.IsNullOrEmpty(model.BillingNewAddress.Address1))
                    //        ModelState.AddModelError("BillingNewAddress.Address1", _localizationService.GetResource("Account.Fields.StreetAddress.Required"));

                    //    if (string.IsNullOrEmpty(model.BillingNewAddress.Address2))
                    //        ModelState.AddModelError("BillingNewAddress.Address2", _localizationService.GetResource("Account.Fields.StreetAddress2.Required"));

                    //    if (string.IsNullOrEmpty(model.BillingNewAddress.City))
                    //        ModelState.AddModelError("BillingNewAddress.City", _localizationService.GetResource("Account.Fields.City.Required"));

                    //    if (model.BillingNewAddress.CountryId <= 0)
                    //        ModelState.AddModelError("BillingNewAddress.CountryId", _localizationService.GetResource("Address.Fields.Country.Required"));
                    //}

                    //if (!ModelState.IsValid)
                    //{
                    //    //model is not valid. redisplay the form with errors
                    //    var shippingAddressModel = _checkoutModelFactory.PrepareShippingAddressModel(
                    //        selectedCountryId: newAddress.CountryId,
                    //        overrideAttributesXml: customAttributes);
                    //    shippingAddressModel.NewAddressPreselected = true;
                    //    return Json(new
                    //    {
                    //        update_section = new UpdateSectionJsonModel
                    //        {
                    //            name = "shipping",
                    //            html = RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                    //        }
                    //    });
                    //}

                    //_workContext.CurrentCustomer.BillingAddress.Address1 = model.BillingNewAddress.Address1;
                    //_workContext.CurrentCustomer.BillingAddress.Address2 = model.BillingNewAddress.Address2;
                    //_workContext.CurrentCustomer.BillingAddress.City = model.BillingNewAddress.City;
                    //_workContext.CurrentCustomer.BillingAddress.CountryId = model.BillingNewAddress.CountryId;
                    //_workContext.CurrentCustomer.BillingAddress.Country = _countryService.GetCountryById((int)model.BillingNewAddress.CountryId);

                    //_customerService.UpdateCustomer(_workContext.CurrentCustomer);

                    //try to find an address with the same values (don't duplicate records)
                    var address = _addressService.FindAddress(_workContext.CurrentCustomer.Addresses.ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes,
                        newAddress.Civility, newAddress.Nationality,
                        newAddress.NationalityType, newAddress.IdentityCardOrPassport);
                    if (address == null)
                    {
                        address = (Address)newAddress.ToEntity().Clone();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;
                        //little hack here (TODO: find a better solution)
                        //EF does not load navigation properties for newly created entities (such as this "Address").
                        //we have to load them manually 
                        //otherwise, "Country" property of "Address" entity will be null in shipping rate computation methods
                        if (address.CountryId.HasValue)
                            address.Country = _countryService.GetCountryById(address.CountryId.Value);
                        if (address.StateProvinceId.HasValue)
                            address.StateProvince = _stateProvinceService.GetStateProvinceById(address.StateProvinceId.Value);

                        //other null validations
                        if (address.CountryId == 0)
                            address.CountryId = null;
                        if (address.StateProvinceId == 0)
                            address.StateProvinceId = null;
                        //_workContext.CurrentCustomer.Addresses.Add(address);
                        _workContext.CurrentCustomer.CustomerAddressMappings.Add(new CustomerAddressMapping { Address = address });
                    }
                    _workContext.CurrentCustomer.ShippingAddress = address;
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);

                    //shipping is required
                    if (_shippingSettings.ShipToSameAddress && model.ShipToSameAddress && _workContext.CurrentCustomer.BillingAddress.Country != null && _workContext.CurrentCustomer.BillingAddress.Country.AllowsShipping)
                    {
                        //ship to the same address
                        //_workContext.CurrentCustomer.ShippingAddress = _workContext.CurrentCustomer.BillingAddress;
                        _workContext.CurrentCustomer.ShippingAddress.Address1 = model.BillingNewAddress.Address1;
                        _workContext.CurrentCustomer.ShippingAddress.Address2 = model.BillingNewAddress.Address2;
                        _workContext.CurrentCustomer.ShippingAddress.City = model.BillingNewAddress.City;
                        _workContext.CurrentCustomer.ShippingAddress.CountryId = model.BillingNewAddress.CountryId;

                        _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                        //reset selected shipping method (in case if "pick up in store" was selected)
                        _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, _storeContext.CurrentStore.Id);
                        _genericAttributeService.SaveAttribute<PickupPoint>(_workContext.CurrentCustomer, NopCustomerDefaults.SelectedPickupPointAttribute, null, _storeContext.CurrentStore.Id);
                        //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 
                        return OpcLoadStepAfterShippingAddress(cart);
                    }
                }
                var jsonReturn = OpcLoadStepAfterShippingAddress(cart);
                
                // order palce with 0
                var isPaymentWorkflowRequired = _orderProcessingService.IsPaymentWorkflowRequired(cart, false);
                if (!isPaymentWorkflowRequired)
                    return OpcConfirmOrder();

                return jsonReturn;
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public virtual IActionResult OpcSavePaymentMethod(string paymentmethod, CheckoutPaymentMethodModel model)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(_localizationService.GetResource("Checkout.Disabled"));

                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

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

                //Check whether payment workflow is required
                var isPaymentWorkflowRequired = _orderProcessingService.IsPaymentWorkflowRequired(cart);
                if (!isPaymentWorkflowRequired)
                {
                    //payment is not required
                    _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute, null, _storeContext.CurrentStore.Id);

                    var confirmOrderModel = _checkoutModelFactory.PrepareConfirmOrderModel(cart);

                    return OpcConfirmOrder();
                    //return Json(new
                    //{
                    //    update_section = new UpdateSectionJsonModel
                    //    {
                    //        name = "confirm-order",
                    //        html = RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                    //    },
                    //    goto_section = "confirm_order"
                    //});
                }

                var paymentMethodInst = _paymentService.LoadPaymentMethodBySystemName(paymentmethod);
                if (paymentMethodInst == null ||
                    !_paymentService.IsPaymentMethodActive(paymentMethodInst) ||
                    !_pluginFinder.AuthenticateStore(paymentMethodInst.PluginDescriptor, _storeContext.CurrentStore.Id) ||
                    !_pluginFinder.AuthorizedForUser(paymentMethodInst.PluginDescriptor, _workContext.CurrentCustomer))
                    throw new Exception("Selected payment method can't be parsed");

                //save
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, paymentmethod, _storeContext.CurrentStore.Id);

                //Comment this line becauase of the currnet payment method is COD or redirect payment.
                //return OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);

                //skip payment info page
                var paymentInfo = new ProcessPaymentRequest();

                //session save
                HttpContext.Session.Set("OrderPaymentInfo", paymentInfo);

                return OpcConfirmOrder();
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public virtual IActionResult OpcConfirmOrder()
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(_localizationService.GetResource("Checkout.Disabled"));

                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                //prevent 2 orders being placed within an X seconds time frame
                if (!IsMinimumOrderPlacementIntervalValid(_workContext.CurrentCustomer))
                    throw new Exception(_localizationService.GetResource("Checkout.MinOrderPlacementInterval"));

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

                processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);
                var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };

                    var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(placeOrderResult.PlacedOrder.PaymentMethodSystemName);
                    if (paymentMethod == null)
                        //payment method could be null if order total is 0
                        //success
                        return Json(new { success = 1, orderId = placeOrderResult.PlacedOrder.Id });

                    if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                    {
                        //Redirection will not work because it's AJAX request.
                        //That's why we don't process it here (we redirect a user to another page where he'll be redirected)

                        //redirect
                        return Json(new
                        {
                            redirect = $"{_webHelper.GetStoreLocation()}checkout/OpcCompleteRedirectionPayment"
                        });
                    }

                    _paymentService.PostProcessPayment(postProcessPaymentRequest);
                    //success
                    return Json(new { success = 1, orderId = placeOrderResult.PlacedOrder.Id });
                }

                //error
                var confirmOrderModel = new CheckoutConfirmModel();
                foreach (var error in placeOrderResult.Errors)
                    confirmOrderModel.Warnings.Add(error);

                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "confirm-order",
                        html = RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                    },
                    goto_section = "confirm_order"
                });
            }
            catch (Exception exc)
            {
                _logger.Warning(exc.Message, exc, _workContext.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }
    }
}