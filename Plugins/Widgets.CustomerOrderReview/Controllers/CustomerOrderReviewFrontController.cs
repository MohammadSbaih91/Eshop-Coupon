using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Data;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Widgets.CustomerOrderReview.Models;

namespace Widgets.CustomerOrderReview.Controllers
{
    public class CustomerOrderReviewFrontController : BaseController
    {
        private readonly IRepository<Domain.CustomerOrderReview> _customerOrderReviewRepository;
        private const string PluginPath ="~/Plugins/Widgets.CustomerOrderReview/Views/";

        public CustomerOrderReviewFrontController(IRepository<Domain.CustomerOrderReview> customerOrderReviewRepository)
        {
            _customerOrderReviewRepository = customerOrderReviewRepository;
        }

        [HttpPost, PublicAntiForgery]
        public IActionResult WriteReview(CustomerOrderReviewModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var existingReview = _customerOrderReviewRepository.Table.FirstOrDefault(cr =>
                cr.OrderId == model.OrderId
                && cr.CustomerId == model.CustomerId
                && cr.StoreId == model.StoreId);

            if (existingReview != null)
            {
                existingReview.Rate1 = model.Rate1;
                existingReview.Rate2 = model.Rate2;
                existingReview.Rate3 = model.Rate3;
                existingReview.Rate4 = model.Rate4;
                existingReview.Feedback = model.Feedback;
                existingReview.CustomerOrderReviewType = model.CustomerOrderReviewType;
                existingReview.OrderId = model.OrderId;
                existingReview.CustomerId = model.CustomerId;
                existingReview.StoreId = model.StoreId;
                existingReview.CreatedOnUtc = DateTime.UtcNow;
                _customerOrderReviewRepository.Update(existingReview);
            }
            else
            {
                var review = new Domain.CustomerOrderReview
                {
                    Rate1 = model.Rate1,
                    Rate2 = model.Rate2,
                    Rate3 = model.Rate3,
                    Rate4 = model.Rate4,
                    Feedback = model.Feedback,
                    CustomerOrderReviewType = model.CustomerOrderReviewType,
                    OrderId = model.OrderId,
                    CustomerId = model.CustomerId,
                    StoreId = model.StoreId,
                    CreatedOnUtc = DateTime.UtcNow
                };
                _customerOrderReviewRepository.Insert(review);
            }

            return View($"{PluginPath}WriteReview.cshtml",
                new CustomerOrderReviewModel());
        }
    }
}