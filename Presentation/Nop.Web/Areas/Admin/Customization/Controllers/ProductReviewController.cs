using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.ExportImport.Help;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.ExportImport.Help;
using Nop.Core;
using Nop.Web.Framework.Controllers;
using Nop.Services.Customers;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class ProductReviewController
    {
        private readonly ICustomerService _customerService = EngineContext.Current.Resolve<ICustomerService>();
        #region Methods
        [HttpPost]
        [FormValueRequired("exportexcel-all")]
        public virtual IActionResult ExportProductReviewExcelAll()
        {
            try
            {
                var productReviews = _productService.GetAllProductReviews(0, true);
                var properties = new[]
                {
                    new PropertyByName<ProductReview>("ProductName", p => {
                        var name =  _productService.GetProductById(p.ProductId).Name;
                        return name;
                        }),
                    new PropertyByName<ProductReview>("Customer", p => {
                        var name =  _customerService.GetCustomerById(p.CustomerId).Username;
                        return name;
                        }),

                        new PropertyByName<ProductReview>("Rating", pr => pr.Rating),
                        new PropertyByName<ProductReview>("IsApproved", pr => pr.IsApproved),
                };
                var productRatingList = productReviews.ToList();
                var bytes = new PropertyManager<ProductReview>(properties, _catalogSettings).ExportToXlsx(productRatingList);
                return File(bytes, MimeTypes.TextXlsx, "productReview.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }
        #endregion
    }
}
