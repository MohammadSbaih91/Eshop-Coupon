using Nop.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.AppointmentBooking.Models;
using Nop.Plugin.Misc.AppointmentBooking.Services;
using Nop.Core;
using System.Linq;
using Nop.Plugin.Misc.AppointmentBooking.Domains;
using System;
using Nop.Services.Orders;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Newtonsoft.Json;
using Nop.Core.Domain.Orders;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web;
using Nop.Services.Common;
using Nop.Services.Shipping;
using Nop.Services.Configuration;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Misc.AppointmentBooking.Controllers
{
    public class AppointmentBookingController : BasePublicController
    {
        #region Fields
        private readonly IAppointmentService _appointmentService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IAddressService _addressService;
        private readonly IShippingService _shippingService;
        private readonly AppointmentBookingSettings _appointmentBookingSettings;
        #endregion

        #region Ctor
        public AppointmentBookingController(IAppointmentService appointmentService,
            IWorkContext workContext,
            IOrderService orderService,
            ILocalizationService localizationService,
            ILogger logger,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IAddressService addressService,
            IShippingService shippingService,
            AppointmentBookingSettings appointmentBookingSettings)
        {
            _appointmentService = appointmentService;
            _workContext = workContext;
            _orderService = orderService;
            _localizationService = localizationService;
            _logger = logger;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _addressService = addressService;
            _shippingService = shippingService;
            _appointmentBookingSettings = appointmentBookingSettings;
        }
        #endregion

        #region Methods

        public virtual IActionResult Appointment(int orderid, bool isChangeAppointent = false)
        {
            var appointment = _appointmentService.GetBookedAppointmentByOrderId(orderid);
            if (!isChangeAppointent && appointment != null && !string.IsNullOrEmpty(appointment.TicketNumber))
            {
                //var order1 = _orderService.GetOrderById(appointment.OrderId);
                //var appBookedCustomerNotificationQueuedEmailIds = _appointmentService.SendAppointmentBookedCustomerNotification(appointment, _workContext.WorkingLanguage.Id);
                //if (appBookedCustomerNotificationQueuedEmailIds.Any())
                //{
                //    order1.OrderNotes.Add(new OrderNote
                //    {
                //        Note = $"\"Appointment Booked\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", appBookedCustomerNotificationQueuedEmailIds)}.",
                //        DisplayToCustomer = false,
                //        CreatedOnUtc = DateTime.UtcNow
                //    });

                //    _orderService.UpdateOrder(order1);
                //}
                return RedirectToAction("BookAppointmentCompleted", new { orderId = orderid });
            }

            //var order = _orderService.GetOrderById(orderid);

            //if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
            //{
            //    return RedirectToRoute("HomePage");
            //}

            ViewBag.OrderId = orderid;
            return View();
        }

        public virtual IActionResult AppointmentDrawer(int addressId, string phoneNumber)
        {
            ViewBag.OrderId = 0;
            ViewBag.AddressId = addressId;
            ViewBag.PhoneNumber = phoneNumber;
            return Json(new { html = RenderPartialViewToString("_AppointmentBooking") });
        }

        public virtual IActionResult AppointmentStoreList(string currentLatitude, string currentLongitude)
        {

            var model = _appointmentService.GetBranchAroundMe(currentLatitude, currentLongitude, _workContext.WorkingLanguage.UniqueSeoCode);
            //model.Branches = model.Branches.Where(p => p.IsWorkingNow).ToList();
            var branches = _appointmentService.GetAppointmentBranchList();
            model.AvailableAppointmentBranch = branches.Select(branch => new SelectListItem
            {
                Text = _localizationService.GetLocalized(branch, mt => mt.Name),
                Value = branch.Latitude + "," + branch.Longitude,
                Selected = currentLatitude + "," + currentLongitude == branch.Latitude + "," + branch.Longitude
            }).ToList();

            return Json(new { html = RenderPartialViewToString("_AppointmentStoreList", model) });
        }

        public virtual IActionResult AppointmentStoreDetail(Branch branch, string openUntil)
        {
            var service = _appointmentService.GetServicesByBranchId(branch.Id.ToString(), _workContext.WorkingLanguage.UniqueSeoCode);
            var availableDays = _appointmentService.GetAvailableDaysToTakeAppointment(branch.Id, service.Id, _workContext.WorkingLanguage.UniqueSeoCode);
            var model = new StoreDetailModel()
            {
                Branch = branch,
                AvailableDaysToTakeAppointment = availableDays,
                ServiceId = service.Id,
                OpenUntil = openUntil
            };

            return Json(new { html = RenderPartialViewToString("_AppointmentStoreDetail", model) });
        }

        public virtual IActionResult AppointmentTime(Branch branch, string appointmentDay, int orderId)
        {
            var model = new StoreDetailModel();
            if (branch == null)
            {
                return Json(new { error = 1, message = _localizationService.GetResource("Plugins.Misc.AppointmentBooking.ErrorMessage.BranchNotLoad") });
            }
            //_logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "branch", JsonConvert.SerializeObject(branch));

            var service = _appointmentService.GetServicesByBranchId(branch.Id.ToString(), _workContext.WorkingLanguage.UniqueSeoCode);
            //_logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "Service", JsonConvert.SerializeObject(service));
            if (service != null && service.Id == null)
            {
                return Json(new { error = 1, message = _localizationService.GetResource("Plugins.Misc.AppointmentBooking.ErrorMessage.ServiceNotLoad") });
            }

            var availableDays = _appointmentService.GetAvailableDaysToTakeAppointment(branch.Id, service.Id, _workContext.WorkingLanguage.UniqueSeoCode);
            //_logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "availableDays", availableDays.ToJson());

            if (availableDays != null && availableDays.GetAvailableDaysToTakeAppointmentResult != null && availableDays.GetAvailableDaysToTakeAppointmentResult.Code != 0)
            {
                model.Code = availableDays.GetAvailableDaysToTakeAppointmentResult.Code;
                model.Description = availableDays.GetAvailableDaysToTakeAppointmentResult.Description;
            }
            else
            {
                if (string.IsNullOrEmpty(appointmentDay))
                    appointmentDay = availableDays.ValidAppointmentDays.FirstOrDefault();

                var availableTimes = _appointmentService.GetDayAvailableTimesToTakeAppointment(appointmentDay, branch.Id, service.Id);
                //_logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "availableTimes", availableTimes.ToJson());
                if (availableTimes != null && availableTimes.GetDayAvailableTimesToTakeAppointmentResult != null && availableTimes.GetDayAvailableTimesToTakeAppointmentResult.Code != 0)
                {
                    model.Code = availableTimes.GetDayAvailableTimesToTakeAppointmentResult.Code;
                    model.Description = availableTimes.GetDayAvailableTimesToTakeAppointmentResult.Description;
                }
                else
                {
                    model.OrderId = orderId;
                    model.Branch = branch;
                    model.AvailableTimesToTakeAppointment = availableTimes;
                    model.AvailableDaysToTakeAppointment = availableDays;
                    model.ServiceId = service.Id;
                    //OpenUntil = openUntil,
                    model.SelectAppointmentDate = appointmentDay;
                }
            }
            return Json(new { html = RenderPartialViewToString("_AppointmentTime", model), error = 0 });
        }

        public virtual IActionResult BookAppointmentCompleted(int orderId)
        {
            var appointment = _appointmentService.GetBookedAppointmentByOrderId(orderId);
            var model = new ConfirmAppointment()
            {
                BookAppointmentRequest = new BookAppointmentRequest()
                {
                    OrderId = appointment.OrderId,
                    AppointmentDay = appointment.AppointmentDay,
                    SelectedAppointmentTime = appointment.AppointmentTime,
                    BranchID = appointment.BranchID,
                    ServiceID = appointment.ServiceID,
                    x_wassup_msisdn = appointment.x_wassup_msisdn,
                    SelectedStoreName = appointment.SelectedStoreName
                },
                BookAppointmentResponse = JsonConvert.DeserializeObject<BookAppointmentResponse>(appointment.JsonResponce)
            };
            return View(model);
        }

        [HttpPost]
        public virtual JsonResult BookAppointmentCompleted(BookAppointmentRequest bookAppointmentRequest, bool isDrawer = false)
        {
            if (bookAppointmentRequest == null)
                return Json(new
                {
                    message = "Appointment is not booked",
                    url = Url.Action("Index", "Home"),
                    isDrawer = isDrawer
                });

            var phoneNumber = "";
            var order = _orderService.GetOrderById(bookAppointmentRequest.OrderId);
            if (order != null)
            {
                phoneNumber = order.BillingAddress.PhoneNumber;
                bookAppointmentRequest.x_wassup_msisdn = order.BillingAddress.PhoneNumber;
            }
            else if (bookAppointmentRequest.AddressId > 0)
            {
                var address = _addressService.GetAddressById(bookAppointmentRequest.AddressId);
                phoneNumber = address?.PhoneNumber;
                bookAppointmentRequest.x_wassup_msisdn = address.PhoneNumber;
            }
            else if (!string.IsNullOrEmpty(bookAppointmentRequest.PhoneNumber))
            {
                phoneNumber = bookAppointmentRequest.PhoneNumber;
                bookAppointmentRequest.x_wassup_msisdn = bookAppointmentRequest.PhoneNumber;
            }
            var appointment = _appointmentService.BookAppointment(bookAppointmentRequest, _workContext.WorkingLanguage.UniqueSeoCode);

            //_logger.InsertLog(Core.Domain.Logging.LogLevel.Information, "appointmentBooking", appointment.ToJson());
            if (appointment.TakeAppointmentResult.Code == 0 && !string.IsNullOrEmpty(appointment.AppointmentTicketInfo.TicketNumber))
            {
                var bookedAppointment = new BookedAppointment()
                {
                    OrderId = bookAppointmentRequest.OrderId,
                    AppointmentDay = bookAppointmentRequest.AppointmentDay,
                    AppointmentTime = bookAppointmentRequest.SelectedAppointmentTime,
                    BranchID = bookAppointmentRequest.BranchID,
                    ServiceID = bookAppointmentRequest.ServiceID,
                    x_wassup_msisdn = phoneNumber,
                    TicketNumber = appointment.AppointmentTicketInfo.TicketNumber,
                    JsonResponce = appointment.ToJson(),
                    CreatedOnUTC = DateTime.UtcNow,
                    SelectedStoreName = bookAppointmentRequest.SelectedStoreName
                };
                _appointmentService.InsertBookedAppointment(bookedAppointment);

                //save bookapointmentId in genericAttribute
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                    EShopHelper.BookAppintmentId, bookedAppointment.Id, _storeContext.CurrentStore.Id);

                //var appBookedCustomerNotificationQueuedEmailIds = _appointmentService.SendAppointmentBookedCustomerNotification(bookedAppointment, _workContext.WorkingLanguage.Id);
                //if (appBookedCustomerNotificationQueuedEmailIds.Any())
                //{
                //    order.OrderNotes.Add(new OrderNote
                //    {
                //        Note = $"\"Appointment Booked\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", appBookedCustomerNotificationQueuedEmailIds)}.",
                //        DisplayToCustomer = false,
                //        CreatedOnUtc = DateTime.UtcNow
                //    });

                //    _orderService.UpdateOrder(order);
                //}

                var model = new ConfirmAppointment()
                {
                    BookAppointmentRequest = bookAppointmentRequest,
                    BookAppointmentResponse = appointment
                };

                //isDrawer true is from cart page
                if (isDrawer)
                {
                    _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, _storeContext.CurrentStore.Id);

                    var pickupPoints = _shippingService.GetPickupPoints(_workContext.CurrentCustomer.BillingAddress,
                        _workContext.CurrentCustomer, null, _storeContext.CurrentStore.Id).PickupPoints.ToList();
                    var selectedPoint = pickupPoints.FirstOrDefault(x => x.Id.Equals(_appointmentBookingSettings.PickUpInStoreId.ToString()));

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

                }

                return Json(new
                {
                    message = "",
                    url = Url.Link("Appointment", new { orderid = bookAppointmentRequest.OrderId, isChangeAppointent = false }),
                    isDrawer = isDrawer
                });
            }
            else
            {
                _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, $"Appointment is not Booked on order {bookAppointmentRequest.OrderId}");
                return Json(new
                {
                    message = appointment.TakeAppointmentResult.Description,
                    url = "",
                    isDrawer = isDrawer
                });
                //return RedirectToAction("Appointment", new { orderid = bookAppointmentRequest.OrderId, isChangeAppointent= false });
            }
        }

        #endregion
    }
}
