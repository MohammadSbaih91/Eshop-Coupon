using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class ProductController
    {
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Edit(ProductModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = _productService.GetProductById(model.Id);
            if (product == null || product.Deleted)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            //check if the product quantity has been changed while we were editing the product
            //and if it has been changed then we show error notification
            //and redirect on the editing page without data saving
            if (product.StockQuantity != model.LastStockQuantity)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Catalog.Products.Fields.StockQuantity.ChangedWarning"));
                return RedirectToAction("Edit", new { id = product.Id });
            }

            if (ModelState.IsValid)
            {
                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null)
                    model.VendorId = _workContext.CurrentVendor.Id;

                //we do not validate maximum number of products per vendor when editing existing products (only during creation of new products)
                //vendors cannot edit "Show on home page" property
                if (_workContext.CurrentVendor != null && model.ShowOnHomePage != product.ShowOnHomePage)
                    model.ShowOnHomePage = product.ShowOnHomePage;

                //some previously used values
                var prevTotalStockQuantity = _productService.GetTotalStockQuantity(product);
                var prevDownloadId = product.DownloadId;
                var prevSampleDownloadId = product.SampleDownloadId;
                var previousStockQuantity = product.StockQuantity;
                var previousWarehouseId = product.WarehouseId;

                
                // TODO : PANKAJ This feature is added by seprated tab so this will get it from DB direct to MAP
                model.RequiredAnyOneFromOtherProductIds = product.RequiredAnyOneFromOtherProductIds;
                model.RequireAnyOneFromOtherProducts = product.RequireAnyOneFromOtherProducts;

                //product
                product = model.ToEntity(product);

                product.UpdatedOnUtc = DateTime.UtcNow;
                _productService.UpdateProduct(product);

                //search engine name
                model.SeName = _urlRecordService.ValidateSeName(product, model.SeName, product.Name, true);
                _urlRecordService.SaveSlug(product, model.SeName, 0);

                //locales
                UpdateLocales(product, model);

                //tags
                _productTagService.UpdateProductTags(product, ParseProductTags(model.ProductTags));

                //warehouses
                SaveProductWarehouseInventory(product, model);

                //categories
                SaveCategoryMappings(product, model);

                //manufacturers
                SaveManufacturerMappings(product, model);

                //ACL (customer roles)
                SaveProductAcl(product, model);

                //stores
                SaveStoreMappings(product, model);

                //discounts
                SaveDiscountMappings(product, model);

                //picture seo names
                UpdatePictureSeoNames(product);

                //back in stock notifications
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    product.BackorderMode == BackorderMode.NoBackorders &&
                    product.AllowBackInStockSubscriptions &&
                    _productService.GetTotalStockQuantity(product) > 0 &&
                    prevTotalStockQuantity <= 0 &&
                    product.Published &&
                    !product.Deleted)
                {
                    _backInStockSubscriptionService.SendNotificationsToSubscribers(product);
                }

                //delete an old "download" file (if deleted or updated)
                if (prevDownloadId > 0 && prevDownloadId != product.DownloadId)
                {
                    var prevDownload = _downloadService.GetDownloadById(prevDownloadId);
                    if (prevDownload != null)
                        _downloadService.DeleteDownload(prevDownload);
                }

                //delete an old "sample download" file (if deleted or updated)
                if (prevSampleDownloadId > 0 && prevSampleDownloadId != product.SampleDownloadId)
                {
                    var prevSampleDownload = _downloadService.GetDownloadById(prevSampleDownloadId);
                    if (prevSampleDownload != null)
                        _downloadService.DeleteDownload(prevSampleDownload);
                }

                //quantity change history
                if (previousWarehouseId != product.WarehouseId)
                {
                    //warehouse is changed 
                    //compose a message
                    var oldWarehouseMessage = string.Empty;
                    if (previousWarehouseId > 0)
                    {
                        var oldWarehouse = _shippingService.GetWarehouseById(previousWarehouseId);
                        if (oldWarehouse != null)
                            oldWarehouseMessage = string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.EditWarehouse.Old"), oldWarehouse.Name);
                    }

                    var newWarehouseMessage = string.Empty;
                    if (product.WarehouseId > 0)
                    {
                        var newWarehouse = _shippingService.GetWarehouseById(product.WarehouseId);
                        if (newWarehouse != null)
                            newWarehouseMessage = string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.EditWarehouse.New"), newWarehouse.Name);
                    }

                    var message = string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.EditWarehouse"), oldWarehouseMessage, newWarehouseMessage);

                    //record history
                    _productService.AddStockQuantityHistoryEntry(product, -previousStockQuantity, 0, previousWarehouseId, message);
                    _productService.AddStockQuantityHistoryEntry(product, product.StockQuantity, product.StockQuantity, product.WarehouseId, message);
                }
                else
                {
                    _productService.AddStockQuantityHistoryEntry(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
                        product.WarehouseId, _localizationService.GetResource("Admin.StockQuantityHistory.Messages.Edit"));
                }

                //activity log
                _customerActivityService.InsertActivity("EditProduct",
                    string.Format(_localizationService.GetResource("ActivityLog.EditProduct"), product.Name), product);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                //selected tab
                SaveSelectedTabName();

                return RedirectToAction("Edit", new { id = product.Id });
            }

            //prepare model
            model = _productModelFactory.PrepareProductModel(model, product, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }


        protected virtual void UpdateLocales(Product product, ProductModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.ShortDescription,
                    localized.ShortDescription,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.FullDescription,
                    localized.FullDescription,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);


                _localizedEntityService.SaveLocalizedValue(product,
                  x => x.OfferDetailsCTA,
                  localized.OfferDetailsCTA,
                  localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(product,
                  x => x.KnowingTerms,
                  localized.KnowingTerms,
                  localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(product,
                  x => x.Conditions,
                  localized.Conditions,
                  localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(product,
                 x => x.ImportantNotes,
                 localized.ImportantNotes,
                 localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(product,
                   x => x.DiscountDesc,
                   localized.DiscountDesc,
                   localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(product,
                  x => x.SpecialPromotionDesc,
                  localized.SpecialPromotionDesc,
                  localized.LanguageId);

                //search engine name
                var seName = _urlRecordService.ValidateSeName(product, localized.SeName, localized.Name, false);
                _urlRecordService.SaveSlug(product, seName, localized.LanguageId);
            }
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult ProductAttributeMappingEdit(ProductAttributeMappingModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product attribute mapping with the specified id
            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(model.Id)
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            //try to get a product with the specified id
            var product = _productService.GetProductById(productAttributeMapping.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
            {
                ErrorNotification(_localizationService.GetResource("This is not your product"));
                return RedirectToAction("List");
            }

            //ensure this attribute is not mapped yet
            if (_productAttributeService.GetProductAttributeMappingsByProductId(product.Id)
                .Any(x => x.ProductAttributeId == model.ProductAttributeId && x.Id != productAttributeMapping.Id))
            {
                //redisplay form
                ErrorNotification(_localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExists"));

                model = _productModelFactory.PrepareProductAttributeMappingModel(model, product, productAttributeMapping, true);

                return View(model);
            }

            productAttributeMapping.ProductAttributeId = model.ProductAttributeId;
            productAttributeMapping.TextPrompt = model.TextPrompt;
            productAttributeMapping.IsRequired = model.IsRequired;
            productAttributeMapping.AttributeControlTypeId = model.AttributeControlTypeId;
            productAttributeMapping.DisplayOrder = model.DisplayOrder;
            productAttributeMapping.ValidationMinLength = model.ValidationMinLength;
            productAttributeMapping.ValidationMaxLength = model.ValidationMaxLength;
            productAttributeMapping.ValidationFileAllowedExtensions = model.ValidationFileAllowedExtensions;
            productAttributeMapping.ValidationFileMaximumSize = model.ValidationFileMaximumSize;
            productAttributeMapping.DefaultValue = model.DefaultValue;
            productAttributeMapping.IsShowButton = model.IsShowButton;

            _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);

            UpdateLocales(productAttributeMapping, model);

            SaveConditionAttributes(productAttributeMapping, model.ConditionModel);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.Updated"));

            if (!continueEditing)
            {
                SaveSelectedTabName("tab-product-attributes");

                return RedirectToAction("Edit", new { id = product.Id });
            }

            //selected tab
            SaveSelectedTabName();

            return RedirectToAction("ProductAttributeMappingEdit", new { id = productAttributeMapping.Id });
        }

        [HttpPost]
        public virtual IActionResult SubsidyProducts(int? productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Json(new SubsidyProductListModel());
            var product = _productService.GetProductById(productId ?? 0);

            if (product == null)
                return Json(new SubsidyProductListModel());

            if (string.IsNullOrEmpty(product.RequiredAnyOneFromOtherProductIds))
                return Json(new SubsidyProductListModel());

            var ids = product.RequiredAnyOneFromOtherProductIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            var productListModel = new List<ProductModel>();
            IList<Product> products = new List<Product>();
            if (ids != null && ids.Length > 0)
            {
                products = _productService.GetProductsByIds(ids);
                foreach (var item in products)
                {
                    productListModel.Add(new ProductModel()
                    {
                        Id = item.Id,
                        Name = item.Name
                    });
                }
            }
            var model = new SubsidyProductListModel
            {
                Data = productListModel,
                Total = products.Count
            };
            return Json(model);
        }

        public virtual IActionResult SubsidyProductAddPopup(int? productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = _productModelFactory.PrepareAddRequiredProductSearchModel(new AddRequiredProductSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult SubsidyProductAddPopupList(AddRequiredProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            //prepare model
            var model = _productModelFactory.PrepareAddRequiredProductListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual IActionResult SubsidyProductAddPopup(AddSubsidyProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.RequiredAnyOneFromOtherProductIds))
                {
                    var list = product.RequiredAnyOneFromOtherProductIds.Split(',',StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    foreach (var item in list)
                    {
                        if (!model.SelectedProductIds.Contains(item))
                            model.SelectedProductIds.Add(item);
                    }
                }
                var subsidyProducts = string.Join(',', model.SelectedProductIds);

                product.RequireAnyOneFromOtherProducts = !string.IsNullOrEmpty(subsidyProducts);
                product.RequiredAnyOneFromOtherProductIds = subsidyProducts;
                _productService.UpdateProduct(product);
            }

            ViewBag.RefreshPage = true;

            return View(new AddRequiredProductSearchModel());
        }
                
        public virtual IActionResult SubsidyProductDelete(int? id, int productId)
        {
            var result = string.Empty;
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);

            if(product != null)
            {
                if (!string.IsNullOrEmpty(product.RequiredAnyOneFromOtherProductIds))
                {
                    var subsidyProducts = product.RequiredAnyOneFromOtherProductIds.Split(',',StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    if (subsidyProducts.Count > 0)
                    {
                        if(id != null && id > 0)
                        {
                            subsidyProducts.Remove((int)id);
                        }
                    }
                    var requiredProductIds = string.Join(',', subsidyProducts);

                    product.RequireAnyOneFromOtherProducts = !string.IsNullOrEmpty(requiredProductIds);
                    product.RequiredAnyOneFromOtherProductIds = requiredProductIds;
                    _productService.UpdateProduct(product);

                }
            }
            return new NullJsonResult();
        }
    }
}
