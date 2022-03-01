
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Discounts;
using Nop.Core.Infrastructure;
using Nop.Services.Customization.Discounts;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Discounts;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class DiscountController : BaseAdminController
    {
        private readonly ICustomDiscountService _customDiscountService = EngineContext.Current.Resolve<ICustomDiscountService>();

        #region Utilities
        public static string GetRandomDiscountCode(int length, string characterSet)
        {
            StringBuilder sb = new StringBuilder();
            Random rnd = new Random();
            for (int i = 0; i < length; i++)
            {
                int index = rnd.Next(characterSet.Length);
                sb.Append(characterSet[index]);
            }
            return sb.ToString();
        }
        #endregion

        #region Methods
        #region Discounts
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(DiscountModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            if (!string.IsNullOrEmpty(model.CouponCode) && _customDiscountService.IsDiscountCouponCodeExist(model.CouponCode))
            {
                ModelState.AddModelError("CouponCode", _localizationService.GetResource("Admin.Promotions.Discounts.Fields.CouponCode.Duplication"));
            }

            if (ModelState.IsValid)
            {
                var discount = model.ToEntity<Discount>();
                _discountService.InsertDiscount(discount);

                //activity log
                _customerActivityService.InsertActivity("AddNewDiscount",
                    string.Format(_localizationService.GetResource("ActivityLog.AddNewDiscount"), discount.Name), discount);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = discount.Id });
            }

            //prepare model
            model = _discountModelFactory.PrepareDiscountModel(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(DiscountModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            //try to get a discount with the specified id
            var discount = _discountService.GetDiscountById(model.Id);
            if (discount == null)
                return RedirectToAction("List");

            if (!string.IsNullOrEmpty(model.CouponCode) && _customDiscountService.IsDiscountCouponCodeExist(model.CouponCode, discount.Id))
            {
                ModelState.AddModelError("CouponCode", _localizationService.GetResource("Admin.Promotions.Discounts.Fields.CouponCode.Duplication"));
            }

            if (ModelState.IsValid)
            {
                var prevDiscountType = discount.DiscountType;
                discount = model.ToEntity(discount);
                _discountService.UpdateDiscount(discount);

                //clean up old references (if changed) and update "HasDiscountsApplied" properties
                if (prevDiscountType == DiscountType.AssignedToCategories && discount.DiscountType != DiscountType.AssignedToCategories)
                {
                    //applied to categories
                    discount.DiscountCategoryMappings.Clear();
                    _discountService.UpdateDiscount(discount);
                }

                if (prevDiscountType == DiscountType.AssignedToManufacturers && discount.DiscountType != DiscountType.AssignedToManufacturers)
                {
                    //applied to manufacturers
                    discount.DiscountManufacturerMappings.Clear();
                    _discountService.UpdateDiscount(discount);
                }

                if (prevDiscountType == DiscountType.AssignedToSkus && discount.DiscountType != DiscountType.AssignedToSkus)
                {
                    //applied to products
                    var products = _discountService.GetProductsWithAppliedDiscount(discount.Id, true);

                    discount.DiscountProductMappings.Clear();
                    _discountService.UpdateDiscount(discount);

                    //update "HasDiscountsApplied" property
                    foreach (var p in products)
                        _productService.UpdateHasDiscountsApplied(p);
                }

                //activity log
                _customerActivityService.InsertActivity("EditDiscount",
                    string.Format(_localizationService.GetResource("ActivityLog.EditDiscount"), discount.Name), discount);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Discounts.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = discount.Id });
            }

            //prepare model
            model = _discountModelFactory.PrepareDiscountModel(model, discount, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Discount Coupon Code
        [HttpPost]
        public IActionResult DiscountCouponCodeList(int discountId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedKendoGridJson();

            var discountCouponCodes = _customDiscountService.GetDiscountCouponCodesList(discountId)
                ?? throw new ArgumentException("No discount coupon codes found with the specified id");
            var discountCodeModel = new List<DiscountCouponCodesModel>();

            foreach (var discountCouponCode in discountCouponCodes)
            {
                discountCodeModel.Add(new DiscountCouponCodesModel()
                {
                    Id = discountCouponCode.Id,
                    CouponCode = discountCouponCode.CouponCode
                });
            }

            var model = new DiscountCouponCodesListModel()
            {
                Data = discountCodeModel,
                Total = discountCouponCodes.Count
            };
            return Json(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("discountCouponCodeGenerate")]
        public IActionResult CreateDiscountCouponCodes(DiscountCouponCodesModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            string characterSet = model.CharacterSet;
            if (string.IsNullOrEmpty(model.CharacterSet))
                characterSet = "abcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*~";
            else
                characterSet.ToLower();

            for (int i = 0; i < model.NoOfCodes; i++)
            {
                var codeLength = model.CodeLength;
                if (!string.IsNullOrEmpty(model.Prefix))
                    codeLength = codeLength - model.Prefix.Length;

                if (!string.IsNullOrEmpty(model.Suffix))
                    codeLength = codeLength - model.Suffix.Length;

                out1:
                var randomCouponCode = GetRandomDiscountCode(codeLength, characterSet);
                randomCouponCode = model.Prefix + randomCouponCode + model.Suffix;
                
                var result = _customDiscountService.CouponCodeIsExist(randomCouponCode);
                if (result)
                    goto out1;

                var discountCouponCode = new DiscountCouponCodes()
                {
                    DiscountId = model.DiscountId,
                    CouponCode = randomCouponCode,
                    IsUsedCount = 0
                };
                _customDiscountService.InsertDiscountCouponCode(discountCouponCode);
            }

            //try to get a discount with the specified id
            var discount = _discountService.GetDiscountById(model.DiscountId);
            if (discount == null)
                return RedirectToAction("List");

            //prepare model
            var discountModel = _discountModelFactory.PrepareDiscountModel(null, discount);

            //selected tab
            SaveSelectedTabName();

            return View(discountModel);
        }

        [HttpPost]
        public IActionResult DeleteDiscountCouponCode(int Id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            //try to get a discount coupon code with the specified id
            var discountCouponCode = _customDiscountService.GetDiscountCouponCodeById(Id)
                ?? throw new ArgumentException("No discount coupon code found with the specified id", nameof(Id));

            _customDiscountService.DeleteDiscountCouponCode(discountCouponCode);

            return new NullJsonResult();
        }
        #endregion
        #endregion
    }
}