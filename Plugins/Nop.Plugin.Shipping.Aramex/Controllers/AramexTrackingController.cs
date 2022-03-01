using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Shipping.Aramex.Models;
using Nop.Plugin.Shipping.Aramex.Services;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Framework.Controllers;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Nop.Plugin.Shipping.Aramex.Controllers
{
    public class AramexTrackingController : BasePublicController
    {
        #region Fields
        private readonly ITrackingService _trackingService;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly AramexSetting _aramexSetting;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPictureService _pictureService;
        private readonly ILogger _logger;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IWorkContext _workContext;
        #endregion

        #region ctor
        public AramexTrackingController(ITrackingService trackingService,
            IOrderService orderService,
            ILocalizationService localizationService,
            AramexSetting aramexSetting,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            IPictureService pictureService,
            ILogger logger,
            IProductAttributeFormatter productAttributeFormatter,
            IWorkContext workContext)
        {
            _trackingService = trackingService;
            _orderService = orderService;
            _localizationService = localizationService;
            _aramexSetting = aramexSetting;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
            _pictureService = pictureService;
            _logger = logger;
            _productAttributeFormatter = productAttributeFormatter;
            _workContext = workContext;
        }
        #endregion

        #region Utilities
        static string ConvertObjectToXMLString(object classObject)
        {
            string xmlString = null;
            XmlSerializer xmlSerializer = new XmlSerializer(classObject.GetType());

            using (MemoryStream memoryStream = new MemoryStream())
            {
                xmlSerializer.Serialize(memoryStream, classObject);
                memoryStream.Position = 0;
                xmlString = new StreamReader(memoryStream).ReadToEnd();
            }
            return xmlString;
        }

        #endregion

        #region Method
        public virtual IActionResult AramexTrackYourOrder()
        {
            var model = new TrackingModel();

            return View(AramexDefault.ViewPath + "AramexTrackYourOrder.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult AramexTrackYourOrder(TrackingModel trackingModel)
        {
            //string errorMessage = "";

            int orderNumber = trackingModel.OrderNumber ?? 0;
            var order = _orderService.GetOrderById(orderNumber);
            if (order != null)
            {
                if (order.BillingAddress.Email == trackingModel.Email)
                {
                    //if (order.OrderStatus == Core.Domain.Orders.OrderStatus.Cancelled)
                    //{
                    //    return RedirectToRoute("AramexOrderCancelled");
                    //}
                    //else
                    //{
                    //    return RedirectToRoute("AramexOrderTracking", new { orderId = order.Id });
                    //}
                    return RedirectToRoute("AramexOrderTracking", new { id = order.Id });
                }
                else
                {
                    ModelState.AddModelError("Email", _localizationService.GetResource("Shipping.Aramex.Tracking.OrderNotMatchWithEmail"));
                }
            }
            else
            {
                ModelState.AddModelError("OrderNumber", _localizationService.GetResource("Shipping.Aramex.Tracking.OrderNumber.incorrects"));
            }


            return View(AramexDefault.ViewPath + "AramexTrackYourOrder.cshtml", trackingModel);
        }

        public virtual IActionResult AramexOrderTracking(int id)
        {
            if (id <= 0)
            {
                return Content("");
            }
            var order = _orderService.GetOrderById(id);
            var shipment = order.Shipments.FirstOrDefault();

            var trackingDetailModel = new TrackingDetailModel()
            {
                Order = order,
                OrderDate = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc)
            };
            if (shipment != null)
            {
                trackingDetailModel.TrackingNumber = shipment.TrackingNumber;
                if(shipment.ExpectedDeliveryDate != null)
                    trackingDetailModel.DeliveryDate = (DateTime)shipment.ExpectedDeliveryDate;

                if (shipment.DeliveryDateUtc != null)
                    trackingDetailModel.DeliveryDate = _dateTimeHelper.ConvertToUserTime((DateTime)shipment.DeliveryDateUtc);

                var clientInfo = new ShipmentTrackingRequestClientInfo()
                {
                    UserName = _aramexSetting.UserName,
                    Password = _aramexSetting.Password,
                    Version = _aramexSetting.Version,
                    AccountNumber = _aramexSetting.AccountNumber,
                    AccountPin = Convert.ToUInt32(_aramexSetting.AccountPin),
                    AccountEntity = _aramexSetting.AccountEntity,
                    AccountCountryCode = _aramexSetting.AccountCountryCode,
                    Source = _aramexSetting.Source
                };
                var transaction = new ShipmentTrackingRequestTransaction()
                {
                    Reference1 = "",
                    Reference2 = "",
                    Reference3 = "",
                    Reference4 = "",
                    Reference5 = ""
                };

                var shipments = new ShipmentTrackingRequestShipments()
                {
                    @string = trackingDetailModel.TrackingNumber
                };

                var shipmentTrackingRequest = new ShipmentTrackingRequest()
                {
                    ClientInfo = clientInfo,
                    Transaction = transaction,
                    Shipments = shipments,
                    GetLastTrackingUpdateOnly = false
                };

                var requestString = ConvertObjectToXMLString(shipmentTrackingRequest);
                try
                {
                    var url = "";
                    var test = requestString.ToString();
                    if (_aramexSetting.UseSandbox)
                        url = "https://ws.aramex.net/ShippingAPI.V2/Tracking/Service_1_0.svc";
                    else
                        url = "http://ws.aramex.net/shippingapi/tracking/service_1_0.svc";

                    //_logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "Aramex Request", requestString);
                    var client = new RestClient(url + "/xml/TrackShipments");
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Content-Type", "application/xml");
                    //request.AddParameter("application/xml", "<?xml version=\"1.0\"?>\r\n<ShipmentTrackingRequest xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://ws.aramex.net/ShippingAPI/v1/\">\r\n  <ClientInfo>\r\n    <UserName>Nabily@aramex.com</UserName>\r\n    <Password>Aramex123$</Password>\r\n    <Version>v1</Version>\r\n    <AccountNumber>20016</AccountNumber>\r\n    <AccountPin>543543</AccountPin>\r\n    <AccountEntity>AMM</AccountEntity>\r\n    <AccountCountryCode>JO</AccountCountryCode>\r\n    <Source>24</Source>\r\n  </ClientInfo>\r\n  <Transaction>\r\n    <Reference1 />\r\n    <Reference2 />\r\n    <Reference3 />\r\n    <Reference4 />\r\n    <Reference5 />\r\n  </Transaction>\r\n  <Shipments xmlns:a=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">\r\n    <a:string>45353001374</a:string>\r\n  </Shipments>\r\n  <GetLastTrackingUpdateOnly>false</GetLastTrackingUpdateOnly>\r\n</ShipmentTrackingRequest>", ParameterType.RequestBody);
                    request.AddParameter("application/xml", requestString, ParameterType.RequestBody);
                    IRestResponse response = client.Execute(request);

                    if (response.IsSuccessful)
                    {
                        //_logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "Aramex responce", response.Content);
                        var xmlDoc1 = new XmlDocument();
                        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(response.Content);
                        MemoryStream stream = new MemoryStream(byteArray);

                        XmlSerializer xmlSerializer1 = new XmlSerializer(typeof(Models.Responce.ShipmentTrackingResponse));
                        trackingDetailModel.ShipmentTrackingResponse = (Models.Responce.ShipmentTrackingResponse)xmlSerializer1.Deserialize(stream);
                    }
                    else
                    {
                        _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, $"Aramex responce :{response.StatusCode}", response.Content + $"Status Desc:{response.StatusDescription}");
                    }


                }
                catch (Exception ex)
                {
                    _logger.Error("Error in Aramex Getting Data", ex);
                }
            }

            var trackingOrderItemModels = new List<TrackingOrderItemModel>();
            //var totalTax = 0M;

            foreach (var item in order.OrderItems)
            {
                var picture = _pictureService.GetPicturesByProductId(item.ProductId, 1).FirstOrDefault();
                var imageUrl = _pictureService.GetPictureUrl(picture, 100);
                var taxValue = item.PriceInclTax - item.PriceExclTax;
                var discTaxValue = item.DiscountAmountInclTax - item.DiscountAmountExclTax;
                taxValue = taxValue - discTaxValue;

                //totalTax = totalTax + taxValue;

                trackingOrderItemModels.Add(new TrackingOrderItemModel()
                {
                    ImageUrl = imageUrl,
                    ProductName = item.Product.Name,
                    ShortDescription = item.Product.ShortDescription,
                    Qty = item.Quantity,
                    TaxValue = taxValue,
                    Tax = _priceFormatter.FormatPrice(taxValue),
                    PriceValue = (item.PriceInclTax - item.DiscountAmountInclTax),
                    Price = _priceFormatter.FormatPrice(item.PriceInclTax - item.DiscountAmountInclTax),
                    PackageId = item.PackageId,
                    AttributeInfo = _productAttributeFormatter.FormatAttributes(item.Product, item.AttributesXml,
                    _workContext.CurrentCustomer, renderPrices: false),
                });

            }
            trackingDetailModel.TrackingOrderItemModels = trackingOrderItemModels;
            trackingDetailModel.TotalQty = trackingOrderItemModels.Sum(p => p.Qty);
            trackingDetailModel.TotalTax = _priceFormatter.FormatPrice(order.OrderTax);
            trackingDetailModel.TotalPrice = _priceFormatter.FormatPrice(order.OrderTotal);


            return View(AramexDefault.ViewPath + "AramexOrderTracking.cshtml", trackingDetailModel);
        }

        public virtual IActionResult AramexOrderCancelled()
        {
            return View(AramexDefault.ViewPath + "AramexOrderCancelled.cshtml");
        }

        public virtual IActionResult AramexOrderActivity()
        {
            return View(AramexDefault.ViewPath + "AramexOrderActivity.cshtml");
        }
        #endregion
    }
}
