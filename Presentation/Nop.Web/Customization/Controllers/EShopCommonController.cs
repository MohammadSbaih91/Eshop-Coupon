using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Web.Controllers;
using Nop.Web.Customization.Models;
using Nop.Web.Customization.Validators.EShopCommon;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Order;

namespace Nop.Web.Customization.Controllers
{
    public partial class EShopCommonController : BasePublicController
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly INopFileProvider _fileProvider;

        public EShopCommonController(IRepository<Order> orderRepository, IOrderModelFactory orderModelFactory, INopFileProvider fileProvider)
        {
            _orderRepository = orderRepository;
            _orderModelFactory = orderModelFactory;
            _fileProvider = fileProvider;
        }

        #region Methods

        [HttpGet]
        public virtual IActionResult AnonymousOrderTrackByIdByEmail() => View(new OrderTrackModel());

        [HttpPost, PublicAntiForgery]
        public virtual IActionResult AnonymousOrderTrackByIdByEmail(OrderTrackModel model)
        {
            try
            {
                var validationResult =
                    new OrderTrackValidator(EngineContext.Current.Resolve<ILocalizationService>()).Validate(model);

                if (validationResult.IsValid)
                {
                    var order = _orderRepository.Table
                        .Include(o=>o.Customer)
                        .Include(o=>o.BillingAddress)
                        .Include(o=>o.ShippingAddress)
                        .FirstOrDefault(o =>o.Id == model.OrderId);

                    if (order != null &&
                        (string.Equals(order?.Customer?.Email,model.Email, StringComparison.InvariantCultureIgnoreCase)||
                        string.Equals(order?.BillingAddress?.Email,model.Email, StringComparison.InvariantCultureIgnoreCase)||
                        string.Equals(order?.ShippingAddress?.Email,model.Email, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        model.Success = true;
                        model.OrderDetailsModel = _orderModelFactory.PrepareOrderDetailsModel(order);
                        model.OrderDetailsModel.OrderNotes =
                            order.OrderNotes.Select(note => new OrderDetailsModel.OrderNote
                            {
                                Id = note.Id,
                                Note = note.Note,
                                CreatedOn = note.CreatedOnUtc
                            }).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                model.Success = false;
                ModelState.AddModelError("error",e.Message);
                return View(model);
            }

            return View(model);
        }


        public IActionResult GoogleSheet(string filename)
        {
            var path = System.IO.Path.Combine(_fileProvider.MapPath("~/wwwroot"), filename);

            if (_fileProvider.FileExists(path))
            {
                var reader = new System.IO.StreamReader(path);
                var text = reader.ReadToEnd();
                return this.Content(text, "text/html");
            }
            else
                return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}