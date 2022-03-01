using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Customization.Orders;
using Nop.Services.Orders;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services.Common;
using Nop.Web.Framework.Mvc;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using Nop.Web.Models;
using Nop.Services.Security;
using Nop.Services.Discounts;
using Nop.Web.Models.ShoppingCart;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Security;
using Nop.Services.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Catalog;
using Nop.Services.Card;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Customers;
using Nop.Services.Customization.Discounts;

namespace Nop.Web.Controllers
{
    public partial class ShoppingCartController
    {
        private readonly ICustomeOrderService _customeOrderService = EngineContext.Current.Resolve<ICustomeOrderService>();
        private readonly IPackagesService _packagesService = EngineContext.Current.Resolve<IPackagesService>();
        private readonly IPackageProductService _packageProductService = EngineContext.Current.Resolve<IPackageProductService>();
        private readonly IProductModelFactory _productModelFactory = EngineContext.Current.Resolve<IProductModelFactory>();
        private readonly ICustomeProductModelFactory _customeProductModelFactory = EngineContext.Current.Resolve<ICustomeProductModelFactory>();
        private readonly ISimCardService _simCardService = EngineContext.Current.Resolve<ISimCardService>();
        private readonly ICustomDiscountService _customDiscountService = EngineContext.Current.Resolve<ICustomDiscountService>();
        #region Utilities

        protected virtual List<int> RequiredOneProductIds(IEnumerable<Product> products)
        {
            var itemIdsToRemove = new List<int>();
            var requiredProductIds = new List<int>();
            while (true)
            {
                var removeItems = products?.ToList() ??
                                  _productService.GetProductsByIds(requiredProductIds.ToArray())
                                      .Where(p => p.RequireOtherProducts || p.RequireAnyOneFromOtherProducts)
                                      .ToList();

                if (!removeItems.Any())
                {
                    itemIdsToRemove.Reverse();
                    return itemIdsToRemove;
                }

                requiredProductIds.Clear();
                foreach (var rp in removeItems)
                {
                    if (rp.RequireOtherProducts)
                        requiredProductIds.AddRange(
                            _productService.ParseRequiredProductIds(rp)
                            .Except(itemIdsToRemove));

                    if (rp.RequireAnyOneFromOtherProducts)
                        requiredProductIds.AddRange(
                            _productService.ParseRequiredAnyOneFromOtherProductIds(rp)
                            .Except(itemIdsToRemove));
                }

                foreach (var requiredProductId in requiredProductIds)
                    if (!itemIdsToRemove.Contains(requiredProductId))
                        itemIdsToRemove.Add(requiredProductId);

                products = null;
            }
        }

        protected ShoppingCartModel LoadCartData(bool? prepareAndDisplayOrderReviewData)
        {
            //if not passed, then create a new model
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var model = new ShoppingCartModel();
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart,
                isEditable: true,
                prepareAndDisplayOrderReviewData: prepareAndDisplayOrderReviewData.GetValueOrDefault());

            foreach (var item in model.Items)
            {
                var product = _productService.GetProductById(item.ProductId);
                item.IsServiceProductAddedToCart = false;
                if (product.IsService)
                {
                    item.IsServiceProductAddedToCart = true;
                }
            }
            return model;
        }

        protected virtual void SaveItemWithPackage(int packageid, ShoppingCartItem updatecartitem, List<string> addToCartWarnings,
            Product product,
            ShoppingCartType cartType, string attributes, decimal customerEnteredPriceConverted,
            DateTime? rentalStartDate,
            DateTime? rentalEndDate, int quantity)
        {
            if (updatecartitem == null)
            {
                //add to the cart
                addToCartWarnings.AddRange(_shoppingCartService.AddToCartWithPackage(packageid, _workContext.CurrentCustomer,
                    product, cartType, _storeContext.CurrentStore.Id,
                    attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity, true));
            }
            else
            {
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(x => x.ShoppingCartType == updatecartitem.ShoppingCartType && x.PackageId == packageid)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                var otherCartItemWithSameParameters = _shoppingCartService.FindShoppingCartItemInTheCart(
                    cart, updatecartitem.ShoppingCartType, product, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate);
                if (otherCartItemWithSameParameters != null &&
                    otherCartItemWithSameParameters.Id == updatecartitem.Id)
                {
                    //ensure it's some other shopping cart item
                    otherCartItemWithSameParameters = null;
                }

                //update existing item
                addToCartWarnings.AddRange(_shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                    updatecartitem.Id, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity + (otherCartItemWithSameParameters?.Quantity ?? 0), true));
                if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
                {
                    //delete the same shopping cart item (the other one)
                    _shoppingCartService.DeleteShoppingCartItem(otherCartItemWithSameParameters);
                }
            }
        }

        protected virtual void SaveItem(ShoppingCartItem updatecartitem, List<string> addToCartWarnings,
           Product product,
           ShoppingCartType cartType, string attributes, decimal customerEnteredPriceConverted,
           DateTime? rentalStartDate,
           DateTime? rentalEndDate, int quantity, decimal subcidyDiscount,int simCardId)
        {
            if (updatecartitem == null)
            {
                //add to the cart
                addToCartWarnings.AddRange(_shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                    product, cartType, _storeContext.CurrentStore.Id,
                    attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity, true, subcidyDiscount, simCardId));
            }
            else
            {
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(x => x.ShoppingCartType == updatecartitem.ShoppingCartType && x.PackageId == 0)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                var otherCartItemWithSameParameters = _shoppingCartService.FindShoppingCartItemInTheCart(
                    cart, updatecartitem.ShoppingCartType, product, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate);
                if (otherCartItemWithSameParameters != null &&
                    otherCartItemWithSameParameters.Id == updatecartitem.Id)
                {
                    //ensure it's some other shopping cart item
                    otherCartItemWithSameParameters = null;
                }

                //update existing item
                addToCartWarnings.AddRange(_shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                    updatecartitem.Id, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity + (otherCartItemWithSameParameters?.Quantity ?? 0), true));
                if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
                {
                    //delete the same shopping cart item (the other one)
                    _shoppingCartService.DeleteShoppingCartItem(otherCartItemWithSameParameters);
                }
            }
        }

        #endregion

        #region Shopping cart
        //add product to cart using AJAX
        //currently we use this method on the product details pages
        [HttpPost, PublicAntiForgery]
        public virtual IActionResult AddProductToCart_Details(int productId, int shoppingCartTypeId,
            IFormCollection form)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("HomePage")
                });
            }

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Json(new
                {
                    success = false,
                    message = "Only simple products could be added to the cart"
                });
            }

            //quantity
            var quantity = 1;
            foreach (var formKey in form.Keys)
                if (formKey.Equals($"addtocart_{productId}.EnteredQuantity",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out quantity);
                    break;
                }

            string devicePackage = form["radioDevicePackage"];
            string enumProductDetail = form["enumProductDetail"];
            int simCardId = 0;
            int.TryParse(form["SelectedAttributeValueId"].ToString(), out int selectedAttributeValueId);
            int.TryParse(form["SelectedSimTypeAttributeValueId"].ToString(), out int selectedSimTypeAttributeValueId);
            int planProductId = 0;
            int.TryParse(form["simCardNumber"].ToString(), out simCardId);
            //if (devicePackage == "ChoosePlanwithdevice")
            //{
            //    //int.TryParse(form["simCardNumber"].ToString(), out simCardNumber);

            //    //if (simCardNumber <= 0)
            //    //{
            //    //    return Json(new
            //    //    {
            //    //        success = false,
            //    //        message = _localizationService.GetResource("ProductDetail.PhoneNumber.SelectSimNumber"),
            //    //        errors = new { simCardNumber = _localizationService.GetResource("ProductDetail.PhoneNumber.SelectSimNumber") }
            //    //    });
            //    //}
            //    int.TryParse(form["PlanProductId"].ToString(), out planProductId);
            //    AddProductToCartPlan(planProductId, shoppingCartTypeId, 1, selectedAttributeValueId: selectedAttributeValueId);
            //}

            //var productTemplateViewPath = _productModelFactory.PrepareProductTemplateViewPath(product);
            //if (productTemplateViewPath == "ProductTemplate.Simple.FixedLine")
            //{
            //    int.TryParse(form["PlanProductId"].ToString(), out planProductId);
            //    AddProductToCartPlan(planProductId, shoppingCartTypeId, 1, selectedAttributeValueId: selectedAttributeValueId);
            //}
            //else if (productTemplateViewPath == "ProductTemplate.Simple.Internet")
            //{
            //    int.TryParse(form["PlanProductId"].ToString(), out planProductId);
            //    AddProductToCartPlan(planProductId, shoppingCartTypeId, 1, selectedAttributeValueId: selectedAttributeValueId);
            //}
            int.TryParse(form["PlanProductId"].ToString(), out planProductId);
            decimal subcidyDiscount1 = decimal.Zero;
            decimal subcidyDiscount2 = decimal.Zero;
            var productTemplateViewPath = _productModelFactory.PrepareProductTemplateViewPath(product);
            if (devicePackage == "ChoosePlanwithdevice")
            {
                if (productTemplateViewPath == "ProductTemplate.Simple")
                {
                    var discountAmt = form["hdnSelectedAttributeValuePrice_" + planProductId + ""].ToString();
                    decimal.TryParse(discountAmt, out subcidyDiscount1);
                }
                else
                {
                    if (!string.IsNullOrEmpty(form["hdnSelectedAttributeValuePrice"].ToString()))
                    {
                        subcidyDiscount2 = Convert.ToDecimal(form["hdnSelectedAttributeValuePrice"].ToString());
                    }
                }
            }


            if (planProductId > 0)
            {
                // TODO : Pankaj if 'devicePackage' is not ChoosePlanwithdevice then planproduct is a simcard other wise it's device so device has not SIM
                var simCardIdForPlan = productTemplateViewPath == "ProductTemplate.Simple" ? simCardId : 0;
                   
                var minQtyOfParentProduct = product.OrderMinimumQuantity;
                var shoppingCart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(item => item.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id).Where(p => p.ProductId == planProductId).ToList();

                int planQty = 1;
                var cartQty = shoppingCart.Select(p => p.Quantity).Sum();
                if (quantity > minQtyOfParentProduct)
                    planQty = quantity - cartQty;
                else
                    planQty = minQtyOfParentProduct - cartQty;

                if (planQty > 0)
                    AddProductToCartPlan(planProductId, shoppingCartTypeId, planQty, selectedAttributeValueId: selectedAttributeValueId, 
                        selectedSimTypeAttributeValueId: selectedSimTypeAttributeValueId, subcidyDiscount: subcidyDiscount2, 
                        selectedProductId: productId, productTemplateViewPath: productTemplateViewPath,simCardId: simCardIdForPlan);
            }

            //update existing shopping cart item
            var updatecartitemid = 0;
            foreach (var formKey in form.Keys)
                if (formKey.Equals($"addtocart_{productId}.UpdatedShoppingCartItemId",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out updatecartitemid);
                    break;
                }

            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                //search with the same cart type as specified
                var cart = _workContext.CurrentCustomer.ShoppingCartItems
                    .Where(x => x.ShoppingCartTypeId == shoppingCartTypeId)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //is it this product?
                if (updatecartitem != null && product.Id != updatecartitem.ProductId)
                {
                    return Json(new
                    {
                        success = false,
                        message = "This product does not match a passed shopping cart item identifier"
                    });
                }
            }

            //customer entered price
            var customerEnteredPriceConverted = decimal.Zero;
            if (product.CustomerEntersPrice)
            {
                foreach (var formKey in form.Keys)
                {
                    if (formKey.Equals($"addtocart_{productId}.CustomerEnteredPrice",
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (decimal.TryParse(form[formKey], out decimal customerEnteredPrice))
                            customerEnteredPriceConverted =
                                _currencyService.ConvertToPrimaryStoreCurrency(customerEnteredPrice,
                                    _workContext.WorkingCurrency);
                        break;
                    }
                }
            }

            var addToCartWarnings = new List<string>();

            //product and gift card attributes
            var attributes = ParseProductAttributes(product, form, addToCartWarnings);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.IsRental)
            {
                ParseRentalDates(product, form, out rentalStartDate, out rentalEndDate);
            }

            var cartType = updatecartitem == null
                ? (ShoppingCartType)shoppingCartTypeId
                :
                //if the item to update is found, then we ignore the specified "shoppingCartTypeId" parameter
                updatecartitem.ShoppingCartType;

            // TODO : Pankaj SIM card set 0 because device has not simcard, Sim card is with plan
            var simCardIdForMainProduct = productTemplateViewPath == "ProductTemplate.Simple" ? 0 : simCardId;
            
            int.TryParse(form["PackageId"].ToString(), out int packageId);
            if (packageId > 0)
            {
                SaveItemWithPackage(packageId, updatecartitem, addToCartWarnings, product, cartType, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity);
            }
            else
            {
                SaveItem(updatecartitem, addToCartWarnings, product, cartType, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity, subcidyDiscount1, simCardIdForMainProduct);
            }

            _customeOrderService.UpdateProductTypeInCart(
                shoppingCartType: cartType,
                product: product,
                attributesXml: attributes,
                customProductTypeId: product.CustomProductTypeId,
                devicePackage: devicePackage,
                storeId: _storeContext.CurrentStore.Id);

            //return result
            return GetProductToCartDetails(addToCartWarnings, cartType, product);
        }

        [HttpPost, PublicAntiForgery]
        public virtual IActionResult AddProductToCart_Catalog(int productId, int shoppingCartTypeId,
            int quantity, bool forceredirection = false)
        {
            var cartType = (ShoppingCartType)shoppingCartTypeId;
            var product = _productService.GetProductById(productId);
            if (product == null)
                //no product found
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Json(new
                {
                    redirect = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }))
                });
            }

            //products with "minimum order quantity" more than a specified qty
            if (product.OrderMinimumQuantity > quantity)
            {
                //we cannot add to the cart such products from category pages
                //it can confuse customers. That's why we redirect customers to the product details page
                return Json(new
                {
                    redirect = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }))
                });
            }

            if (product.CustomerEntersPrice)
            {
                //cannot be added to the cart (requires a customer to enter price)
                return Json(new
                {
                    redirect = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }))
                });
            }

            if (product.IsRental)
            {
                //rental products require start/end dates to be entered
                return Json(new
                {
                    redirect = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }))
                });
            }

            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            if (allowedQuantities.Length > 0)
            {
                //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
                return Json(new
                {
                    redirect = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }))
                });
            }

            //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            if (productAttributes.Any(pam => pam.AttributeControlType != AttributeControlType.ReadonlyCheckboxes))
            {
                //product has some attributes. let a customer see them
                return Json(new
                {
                    redirect = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }))
                });
            }

            //creating XML for "read-only checkboxes" attributes
            var attXml = productAttributes.Aggregate(string.Empty, (attributesXml, attribute) =>
            {
                var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                foreach (var selectedAttributeId in attributeValues
                    .Where(v => v.IsPreSelected)
                    .Select(v => v.Id)
                    .ToList())
                {
                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                        attribute, selectedAttributeId.ToString());
                }

                return attributesXml;
            });

            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == cartType)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, cartType, product);
            //if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
            var addToCartWarnings = _shoppingCartService
                .GetShoppingCartItemWarnings(_workContext.CurrentCustomer, cartType,
                    product, _storeContext.CurrentStore.Id, string.Empty,
                    decimal.Zero, null, null, quantityToValidate, false, shoppingCartItem?.Id ?? 0, true, false, false,
                    false);
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //let's display standard warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            //now let's try adding product to the cart (now including product attribute validation, etc)
            addToCartWarnings = _shoppingCartService.AddToCart(customer: _workContext.CurrentCustomer,
                product: product,
                shoppingCartType: cartType,
                storeId: _storeContext.CurrentStore.Id,
                attributesXml: attXml,
                quantity: quantity);
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //but we do not display attribute and gift card warnings here. let's do it on the product details page
                return Json(new
                {
                    redirect = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }))
                });
            }

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist",
                            string.Format(_localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"),
                                product.Name), product);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct || forceredirection)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var updatetopwishlistsectionhtml = string.Format(
                            _localizationService.GetResource("Wishlist.HeaderQuantity"),
                            _workContext.CurrentCustomer.ShoppingCartItems
                                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                                .LimitPerStore(_storeContext.CurrentStore.Id)
                                .Sum(item => item.Quantity));
                        return Json(new
                        {
                            success = true,
                            message = string.Format(
                                _localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"),
                                Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml
                        });
                    }

                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart",
                            string.Format(_localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"),
                                product.Name), product);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var updatetopcartsectionhtml = string.Format(
                            _localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                            _workContext.CurrentCustomer.ShoppingCartItems
                                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                                .LimitPerStore(_storeContext.CurrentStore.Id)
                                .Sum(item => item.Quantity));

                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? this.RenderViewComponentToString("FlyoutShoppingCart")
                            : "";

                        return Json(new
                        {
                            success = true,
                            message = string.Format(
                                _localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"),
                                Url.RouteUrl("ShoppingCart")),
                            updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml
                        });
                    }
            }
        }

        [HttpPost]
        public virtual IActionResult AddProductToCartPlan(int productId, int shoppingCartTypeId,
            int quantity, bool forceredirection = false, int selectedAttributeValueId = 0, int selectedSimTypeAttributeValueId = 0, 
            decimal subcidyDiscount = 0, int selectedProductId = 0, string productTemplateViewPath = null,int simCardId =0)
        {
            var cartType = (ShoppingCartType)shoppingCartTypeId;
            var product = _productService.GetProductById(productId);
            if (product == null)
                //no product found
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Json(new
                {
                    redirect = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }))
                });
            }

            //products with "minimum order quantity" more than a specified qty
            if (product.OrderMinimumQuantity > quantity)
            {
                //we cannot add to the cart such products from category pages
                //it can confuse customers. That's why we redirect customers to the product details page
                return Json(new
                {
                    redirect = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }))
                });
            }

            if (product.CustomerEntersPrice)
            {
                //cannot be added to the cart (requires a customer to enter price)
                return Json(new
                {
                    redirect = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }))
                });
            }

            if (product.IsRental)
            {
                //rental products require start/end dates to be entered
                return Json(new
                {
                    redirect = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }))
                });
            }

            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            if (allowedQuantities.Length > 0)
            {
                //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
                return Json(new
                {
                    redirect = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }))
                });
            }

            //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            //if (productAttributes.Any(pam => pam.AttributeControlType != AttributeControlType.ReadonlyCheckboxes))
            //{
            //    //product has some attributes. let a customer see them
            //    return Json(new
            //    {
            //        redirect = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }))
            //    });
            //}

            //creating XML for "read-only checkboxes" attributes



            //var productAttributeValue = _productAttributeService.GetProductAttributeValueById(selectedAttributeValueId);
            var attXml = productAttributes.Aggregate(string.Empty, (attributesXml, attribute) =>
            {
                var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);

                //isAttribAdded is used if attribute is already added do not added secound time
                bool isAttribAdded = false;
                if (selectedAttributeValueId == 0)
                {
                    foreach (var selectedAttributeId in attributeValues
                        .Where(v => v.IsPreSelected)
                        .Select(v => v.Id)
                        .ToList())
                    {
                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                            attribute, selectedAttributeId.ToString());

                        isAttribAdded = true;
                    }
                }
                else
                {
                    var selectedAttribute = attributeValues.FirstOrDefault(v => v.Id == selectedAttributeValueId);
                    if (selectedAttribute != null)
                    {
                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, selectedAttribute.Id.ToString());

                        isAttribAdded = true;
                    }

                }

                if (!isAttribAdded && productTemplateViewPath == "ProductTemplate.Simple")
                {
                    // Sim Type Attribute 
                    if (selectedSimTypeAttributeValueId == 0)
                    {
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                        {
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                        }
                    }
                    else
                    {
                        var selectedAttribute = attributeValues.FirstOrDefault(v => v.Id == selectedSimTypeAttributeValueId);
                        if (selectedAttribute != null)
                        {
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttribute.Id.ToString());
                        }
                        

                    }
                }

                return attributesXml;
            });


            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == cartType)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, cartType, product);
            //if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
            var addToCartWarnings = _shoppingCartService
                .GetShoppingCartItemWarnings(_workContext.CurrentCustomer, cartType,
                    product, _storeContext.CurrentStore.Id, string.Empty,
                    decimal.Zero, null, null, quantityToValidate, false, shoppingCartItem?.Id ?? 0, true, false, false,
                    false, selectedProductId: selectedProductId);
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //let's display standard warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            //now let's try adding product to the cart (now including product attribute validation, etc)
            addToCartWarnings = _shoppingCartService.AddToCart(customer: _workContext.CurrentCustomer,
                product: product,
                shoppingCartType: cartType,
                storeId: _storeContext.CurrentStore.Id,
                attributesXml: attXml,
                quantity: quantity, subcidyDiscount: subcidyDiscount,simCardId:simCardId);
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //but we do not display attribute and gift card warnings here. let's do it on the product details page
                return Json(new
                {
                    redirect = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }))
                });
            }

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist",
                            string.Format(_localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"),
                                product.Name), product);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct || forceredirection)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var updatetopwishlistsectionhtml = string.Format(
                            _localizationService.GetResource("Wishlist.HeaderQuantity"),
                            _workContext.CurrentCustomer.ShoppingCartItems
                                .Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist)
                                .LimitPerStore(_storeContext.CurrentStore.Id)
                                .Sum(item => item.Quantity));
                        return Json(new
                        {
                            success = true,
                            message = string.Format(
                                _localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"),
                                Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml
                        });
                    }

                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart",
                            string.Format(_localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"),
                                product.Name), product);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var updatetopcartsectionhtml = string.Format(
                            _localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                            _workContext.CurrentCustomer.ShoppingCartItems
                                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                                .LimitPerStore(_storeContext.CurrentStore.Id)
                                .Sum(item => item.Quantity));

                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? this.RenderViewComponentToString("FlyoutShoppingCart")
                            : "";

                        return Json(new
                        {
                            success = true,
                            message = string.Format(
                                _localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"),
                                Url.RouteUrl("ShoppingCart")),
                            updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml
                        });
                    }
            }
        }

        public virtual IActionResult ProductDetails_AttributeChange(int productId, bool validateAttributeConditions,
            bool loadPicture, IFormCollection form)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return new NullJsonResult();

            var errors = new List<string>();
            var attributeXml = ParseProductAttributes(product, form, errors);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.IsRental)
            {
                ParseRentalDates(product, form, out rentalStartDate, out rentalEndDate);
            }

            //sku, mpn, gtin
            var sku = _productService.FormatSku(product, attributeXml);
            var mpn = _productService.FormatMpn(product, attributeXml);
            var gtin = _productService.FormatGtin(product, attributeXml);

            //price
            var price = "";
            var taxRate = decimal.Zero;
            var taxRate2 = decimal.Zero;
            var discountAmount = decimal.Zero;
            var finalProductPrice = decimal.Zero;
            //base price
            var basepricepangv = "";
            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices) && !product.CustomerEntersPrice)
            {
                //we do not calculate price of "customer enters price" option is enabled
                List<DiscountForCaching> scDiscounts;
                var finalPrice = _priceCalculationService.GetUnitPrice(product,
                    _workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart,
                    1, attributeXml, 0,
                    rentalStartDate, rentalEndDate,
                    true, out discountAmount, out scDiscounts);
                var finalPriceWithDiscountBase =
                    _taxService.GetProductPrice(product, finalPrice, 1, out taxRate, out taxRate2);

                if (finalPriceWithDiscountBase > decimal.Zero)
                    finalPriceWithDiscountBase -= discountAmount;
                var finalPriceWithDiscount =
                    _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase,
                        _workContext.WorkingCurrency);
                finalProductPrice = finalPriceWithDiscount;
                price = _priceFormatter.FormatPrice(finalPriceWithDiscount);
                basepricepangv = _priceFormatter.FormatBasePrice(product, finalPriceWithDiscountBase);
            }

            //stock
            var stockAvailability = _productService.FormatStockMessage(product, attributeXml);

            //conditional attributes
            var enabledAttributeMappingIds = new List<int>();
            var disabledAttributeMappingIds = new List<int>();
            if (validateAttributeConditions)
            {
                var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                foreach (var attribute in attributes)
                {
                    var conditionMet = _productAttributeParser.IsConditionMet(attribute, attributeXml);
                    if (conditionMet.HasValue)
                    {
                        if (conditionMet.Value)
                            enabledAttributeMappingIds.Add(attribute.Id);
                        else
                            disabledAttributeMappingIds.Add(attribute.Id);
                    }
                }
            }

            //picture. used when we want to override a default product picture when some attribute is selected
            var pictureFullSizeUrl = string.Empty;
            var pictureDefaultSizeUrl = string.Empty;
            if (loadPicture)
            {
                //first, try to get product attribute combination picture
                var pictureId = _productAttributeParser.FindProductAttributeCombination(product, attributeXml)
                                    ?.PictureId ?? 0;

                //then, let's see whether we have attribute values with pictures
                if (pictureId == 0)
                {
                    pictureId = _productAttributeParser.ParseProductAttributeValues(attributeXml)
                                    .FirstOrDefault(attributeValue => attributeValue.PictureId > 0)?.PictureId ?? 0;
                }

                if (pictureId > 0)
                {
                    var productAttributePictureCacheKey = string.Format(
                        ModelCacheEventConsumer.PRODUCTATTRIBUTE_PICTURE_MODEL_KEY,
                        pictureId, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    var pictureModel = _cacheManager.Get(productAttributePictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(pictureId);
                        return picture == null
                            ? new PictureModel()
                            : new PictureModel
                            {
                                FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                                ImageUrl = _pictureService.GetPictureUrl(picture,
                                    _mediaSettings.ProductDetailsPictureSize)
                            };
                    });
                    pictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                    pictureDefaultSizeUrl = pictureModel.ImageUrl;
                }
            }

            var isFreeShipping = product.IsFreeShipping;
            if (isFreeShipping && !string.IsNullOrEmpty(attributeXml))
            {
                isFreeShipping = _productAttributeParser.ParseProductAttributeValues(attributeXml)
                    .Where(attributeValue =>
                        attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                    .Select(attributeValue => _productService.GetProductById(attributeValue.AssociatedProductId))
                    .All(associatedProduct => associatedProduct == null || !associatedProduct.IsShipEnabled ||
                                              associatedProduct.IsFreeShipping);
            }

            var taxSplitInfo = new TaxSplitInfoModel
            {
                SplitAmount = product.SplitAmount,
                SplitAmount2 = product.SplitAmount2,
                TaxSplit = taxRate,
                TaxSplit2 = taxRate2,
                DiscountAmount = discountAmount
            };

            return Json(new
            {
                gtin,
                mpn,
                sku,
                price,
                finalProductPrice,
                basepricepangv,
                stockAvailability,
                enabledattributemappingids = enabledAttributeMappingIds.ToArray(),
                disabledattributemappingids = disabledAttributeMappingIds.ToArray(),
                pictureFullSizeUrl,
                pictureDefaultSizeUrl,
                isFreeShipping,
                splitValue1 = taxSplitInfo.SplitInfoFormatted,
                splitValue2 = taxSplitInfo.SplitInfoFormatted2,
                AmountWithTax = _priceFormatter.FormatPrice(taxSplitInfo.AmountWithTax, true, false),
                AmountWithoutTax = _priceFormatter.FormatPrice(taxSplitInfo.AmountWithoutTax, true, false),
                TaxAmount = _priceFormatter.FormatPrice(taxSplitInfo.TaxAmount, true, false),
                message = errors.Any() ? errors.ToArray() : null
            });
        }

        [HttpPost]
        public virtual IActionResult ProductDetails_AddPlan(decimal productPrice, decimal planPrice, decimal advancePayment, decimal selectedAttributeValue = 0, bool isPlanWithDevice = false)
        {
            decimal deviceprice = decimal.Zero;
            string simPrice = "";
            if (isPlanWithDevice)
            {
                deviceprice = (planPrice - selectedAttributeValue); //- advancePayment;
                simPrice = EShopHelper.GetPriceFormatting(_priceFormatter.FormatPrice(deviceprice));

            }
            else
            {
                deviceprice = (productPrice - selectedAttributeValue) - advancePayment;
            }

            decimal productFullPrice = (productPrice * 24);
            decimal planFullPrice = (planPrice * 24);
            var formatedProductFullPrice = EShopHelper.GetPriceFormatting(_priceFormatter.FormatPrice(productFullPrice));
            var formatedPlanFullPrice = EShopHelper.GetPriceFormatting(_priceFormatter.FormatPrice(planFullPrice));
            var subTotalPrice = ((productPrice + planPrice) - selectedAttributeValue) - advancePayment;
            var totalPrice = ((productPrice + planPrice) - selectedAttributeValue);
            decimal totalFullPrice = (totalPrice * 24);
            var formatedTotalFullPrice = EShopHelper.GetPriceFormatting(_priceFormatter.FormatPrice(totalFullPrice));
            var price = EShopHelper.GetPriceFormatting(_priceFormatter.FormatPrice(totalPrice));
            var deviceFormatedPrice = EShopHelper.GetPriceFormatting(_priceFormatter.FormatPrice(deviceprice));
            var subTotalFormatedPrice = EShopHelper.GetPriceFormatting(_priceFormatter.FormatPrice(subTotalPrice));

            return Json(new { price, totalPrice, formatedProductFullPrice, formatedPlanFullPrice, formatedTotalFullPrice, deviceFormatedPrice, subTotalFormatedPrice, simPrice });
        }

        public virtual IActionResult CartDrawer(bool? prepareAndDisplayOrderReviewData)
        {
            var model = LoadCartData(prepareAndDisplayOrderReviewData);
            var cartString = RenderPartialViewToString("~/Themes/Eshop2021/Views/ShoppingCart/CartDrawer.cshtml", model);
            //var crossSell = LoadCrossSellData();
            return Json(new { html = cartString });
        }

        public virtual IActionResult CrossSellProduct()
        {
            var _aclService = EngineContext.Current.Resolve<IAclService>();
            var _storeMappingService = EngineContext.Current.Resolve<IStoreMappingService>();
            var _productModelFactory = EngineContext.Current.Resolve<IProductModelFactory>();

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var products = _productService.GetCrosssellProductsByShoppingCart(cart, _shoppingCartSettings.CrossSellsNumber);
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();
            //visible individually
            products = products.Where(p => p.VisibleIndividually).ToList();

            if (!products.Any())
                return Json(new { html = "" });

            //Cross-sell products are displayed on the shopping cart page.
            //We know that the entire shopping cart page is not refresh
            //even if "ShoppingCartSettings.DisplayCartAfterAddingProduct" setting  is enabled.
            //That's why we force page refresh (redirect) in this case
            var model = _productModelFactory.PrepareProductOverviewModels(products, forceRedirectionAfterAddingToCart: false)
                .ToList();

            var cartString = RenderPartialViewToString("~/Themes/Eshop2021/Views/ShoppingCart/CrossSell.cshtml", model);
            return Json(new { html = cartString });
        }

        public virtual IActionResult RemoveCartProduct(int shoppingCartId)
        {
            //Delete Product From Cart
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var itemIdsToRemove = new List<int>();
            itemIdsToRemove.Add(shoppingCartId);

            #region RemoveRequiredProducts

            //auto add required products to remove list for deleted item
            var cartItems = _workContext.CurrentCustomer.ShoppingCartItems;

            var requiredProductIds = RequiredOneProductIds(cartItems
                .Where(sc => itemIdsToRemove.Contains(sc.Id))
                .Select(sc => sc.Product));

            //itemIdsToRemove.AddRange(cartItems.Where(c => requiredProductIds.Contains(c.Product.Id)).Select(c => c.Id));

            foreach (var requiredProductId in requiredProductIds)
            {
                var item = cartItems.FirstOrDefault(c => requiredProductId == c.Product.Id)?.Id;
                if (item.HasValue && !itemIdsToRemove.Contains(item.Value)) itemIdsToRemove.Add(item.Value);
            }
            #endregion

            //get order items with changed quantity
            var itemsWithNewQuantity = cart.Select(item => new
            {
                //try to get a new quantity for the item, set 0 for items to remove
                NewQuantity = itemIdsToRemove.Contains(item.Id) ? 0 : item.Quantity,
                Item = item
            }).Where(item => item.NewQuantity != item.Item.Quantity);

            var orderedCart = itemsWithNewQuantity
                .OrderByDescending(cartItem =>
                      cartItem.NewQuantity < cartItem.Item.Quantity &&
                     ((cartItem.Item.Product?.RequireOtherProducts ?? false) ||
                      (cartItem.Item.Product?.RequireAnyOneFromOtherProducts ?? false)) ||
                      cartItem.NewQuantity > cartItem.Item.Quantity &&
                      cart.Any(item => item.Product != null && item.Product.RequireOtherProducts && _productService
                                           .ParseRequiredProductIds(item.Product).Contains(cartItem.Item.ProductId)) ||
                      cart.Any(item => item.Product != null && item.Product.RequireAnyOneFromOtherProducts && _productService
                                           .ParseRequiredAnyOneFromOtherProductIds(item.Product).Contains(cartItem.Item.ProductId)))
                .ToList();
            // This code is for package
            var currentPackage = cart.Where(p => p.Id == shoppingCartId).FirstOrDefault();
            var packageid = 0;
            if (currentPackage != null)
                packageid = currentPackage.PackageId;

            var count = itemIdsToRemove.Count;
            for (var index = 0; index < count; index++)
            {
                var item = orderedCart.FirstOrDefault(c => itemIdsToRemove[index] == c.Item.Id && c.NewQuantity == 0);
                if (item != null)
                {
                    orderedCart.RemoveAt(orderedCart.IndexOf(item));
                    orderedCart.Insert(index, item);
                }
            }

            //try to update cart items with new quantities and get warnings
            var warnings = orderedCart.Select(cartItem => new
            {
                ItemId = cartItem.Item.Id,
                Warnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                    cartItem.Item.Id, cartItem.Item.AttributesXml, cartItem.Item.CustomerEnteredPrice,
                    cartItem.Item.RentalStartDateUtc, cartItem.Item.RentalEndDateUtc, cartItem.NewQuantity, true)
            }).ToList();

            // This code is for package
            var warning = warnings.Where(wa => wa.Warnings.Any()).ToList();
            if (!warning.Any() && packageid > 0)
            {
                var packageProducts = cart.Where(p => p.PackageId == packageid).ToList();
                if (packageProducts != null && packageProducts.Count() > 0)
                {
                    foreach (var item in packageProducts)
                    {
                        _shoppingCartService.UpdateShoppingCart(_workContext.CurrentCustomer, item.Id);
                    }
                }
            }

            var model = LoadCartData(true);
            //update current warnings
            foreach (var warningItem in warnings.Where(warningItem => warningItem.Warnings.Any()))
            {
                //find shopping cart item model to display appropriate warnings
                var itemModel = model.Items.FirstOrDefault(item => item.Id == warningItem.ItemId);
                if (itemModel != null)
                    itemModel.Warnings = warningItem.Warnings.Concat(itemModel.Warnings).Distinct().ToList();
            }

            var cartString = RenderPartialViewToString("~/Themes/Eshop2021/Views/ShoppingCart/CartDrawer.cshtml", model);
            return Json(new { html = cartString });
        }

        public virtual IActionResult UpdateQuantity(int[] updateCartids, int quantity)
        {
            // Update Cart Quantity

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //get order items with changed quantity
            var itemsWithNewQuantity = cart.Select(item => new
            {
                //try to get a new quantity for the item, set 0 for items to remove
                NewQuantity = updateCartids.Contains(item.Id) ? quantity : item.Quantity,
                Item = item
            }).Where(item => item.NewQuantity != item.Item.Quantity);

            //order cart items
            //first should be items with a reduced quantity and that require other products; or items with an increased quantity and are required for other products
            var orderedCart = itemsWithNewQuantity
                .OrderByDescending(cartItem =>
                      cartItem.NewQuantity < cartItem.Item.Quantity &&
                     ((cartItem.Item.Product?.RequireOtherProducts ?? false) ||
                      (cartItem.Item.Product?.RequireAnyOneFromOtherProducts ?? false)) ||
                      cartItem.NewQuantity > cartItem.Item.Quantity &&
                      cart.Any(item => item.Product != null && item.Product.RequireOtherProducts && _productService
                                           .ParseRequiredProductIds(item.Product).Contains(cartItem.Item.ProductId)) ||
                      cart.Any(item => item.Product != null && item.Product.RequireAnyOneFromOtherProducts && _productService
                                           .ParseRequiredAnyOneFromOtherProductIds(item.Product).Contains(cartItem.Item.ProductId)))
                .ToList();

            var warnings = orderedCart.Select(cartItem => new
            {
                ItemId = cartItem.Item.Id,
                Warnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                    cartItem.Item.Id, cartItem.Item.AttributesXml, cartItem.Item.CustomerEnteredPrice,
                    cartItem.Item.RentalStartDateUtc, cartItem.Item.RentalEndDateUtc, cartItem.NewQuantity, true)
            }).ToList();

            var model = LoadCartData(true);

            //update current warnings
            foreach (var warningItem in warnings.Where(warningItem => warningItem.Warnings.Any()))
            {
                //find shopping cart item model to display appropriate warnings
                var itemModel = model.Items.FirstOrDefault(item => item.Id == warningItem.ItemId);
                if (itemModel != null)
                    itemModel.Warnings = warningItem.Warnings.Concat(itemModel.Warnings).Distinct().ToList();
            }

            var cartString = RenderPartialViewToString("~/Themes/Eshop2021/Views/ShoppingCart/CartDrawer.cshtml", model);
            return Json(new { html = cartString });
        }

        //public virtual IActionResult UpdatePackageQty(string shoppingCartIds, int quantity)
        //{
        //    // Update Cart Quantity

        //    var cart = _workContext.CurrentCustomer.ShoppingCartItems
        //        .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
        //        .LimitPerStore(_storeContext.CurrentStore.Id)
        //        .ToList();

        //    var updateCartids = new List<int>();
        //    if (string.IsNullOrEmpty(shoppingCartIds))
        //    {
        //        var ids = shoppingCartIds.Split(",").Select(Int32.Parse).ToList();
        //        updateCartids.AddRange(ids);
        //    }

        //    //get order items with changed quantity
        //    var itemsWithNewQuantity = cart.Select(item => new
        //    {
        //        //try to get a new quantity for the item, set 0 for items to remove
        //        NewQuantity = updateCartids.Contains(item.Id) ? quantity : item.Quantity,
        //        Item = item
        //    }).Where(item => item.NewQuantity != item.Item.Quantity);

        //    //order cart items
        //    //first should be items with a reduced quantity and that require other products; or items with an increased quantity and are required for other products
        //    var orderedCart = itemsWithNewQuantity
        //        .OrderByDescending(cartItem =>
        //              cartItem.NewQuantity < cartItem.Item.Quantity &&
        //             ((cartItem.Item.Product?.RequireOtherProducts ?? false) ||
        //              (cartItem.Item.Product?.RequireAnyOneFromOtherProducts ?? false)) ||
        //              cartItem.NewQuantity > cartItem.Item.Quantity &&
        //              cart.Any(item => item.Product != null && item.Product.RequireOtherProducts && _productService
        //                                   .ParseRequiredProductIds(item.Product).Contains(cartItem.Item.ProductId)) ||
        //              cart.Any(item => item.Product != null && item.Product.RequireAnyOneFromOtherProducts && _productService
        //                                   .ParseRequiredAnyOneFromOtherProductIds(item.Product).Contains(cartItem.Item.ProductId)))
        //        .ToList();

        //    var warnings = orderedCart.Select(cartItem => new
        //    {
        //        ItemId = cartItem.Item.Id,
        //        Warnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
        //            cartItem.Item.Id, cartItem.Item.AttributesXml, cartItem.Item.CustomerEnteredPrice,
        //            cartItem.Item.RentalStartDateUtc, cartItem.Item.RentalEndDateUtc, cartItem.NewQuantity, true)
        //    }).ToList();

        //    var cartString = LoadCartData(true);
        //    return Json(new { html = cartString });
        //}

        public virtual IActionResult EmptyBasket()
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var cartids = new List<int>();

            foreach (var item in cart)
            {
                cartids.Add(item.Id);
            }
            #region RemoveRequiredProducts

            //auto add required products to remove list for deleted item
            var cartItems = _workContext.CurrentCustomer.ShoppingCartItems;

            var requiredProductIds = RequiredOneProductIds(cartItems
                .Where(sc => cartids.Contains(sc.Id))
                .Select(sc => sc.Product));

            //itemIdsToRemove.AddRange(cartItems.Where(c => requiredProductIds.Contains(c.Product.Id)).Select(c => c.Id));

            foreach (var requiredProductId in requiredProductIds)
            {
                var item = cartItems.FirstOrDefault(c => requiredProductId == c.Product.Id)?.Id;
                if (item.HasValue && !cartids.Contains(item.Value)) cartids.Add(item.Value);
            }
            #endregion

            //get order items with changed quantity
            var itemsWithNewQuantity = cart.Select(item => new
            {
                //try to get a new quantity for the item, set 0 for items to remove
                NewQuantity = cartids.Contains(item.Id) ? 0 : item.Quantity,
                Item = item
            }).Where(item => item.NewQuantity != item.Item.Quantity);

            var orderedCart = itemsWithNewQuantity
                .OrderByDescending(cartItem =>
                      cartItem.NewQuantity < cartItem.Item.Quantity &&
                     ((cartItem.Item.Product?.RequireOtherProducts ?? false) ||
                      (cartItem.Item.Product?.RequireAnyOneFromOtherProducts ?? false)) ||
                      cartItem.NewQuantity > cartItem.Item.Quantity &&
                      cart.Any(item => item.Product != null && item.Product.RequireOtherProducts && _productService
                                           .ParseRequiredProductIds(item.Product).Contains(cartItem.Item.ProductId)) ||
                      cart.Any(item => item.Product != null && item.Product.RequireAnyOneFromOtherProducts && _productService
                                           .ParseRequiredAnyOneFromOtherProductIds(item.Product).Contains(cartItem.Item.ProductId)))
                .ToList();

            var count = cartids.Count;
            for (var index = 0; index < count; index++)
            {
                var item = orderedCart.FirstOrDefault(c => cartids[index] == c.Item.Id && c.NewQuantity == 0);
                orderedCart.RemoveAt(orderedCart.IndexOf(item));
                orderedCart.Insert(index, item);
            }

            //try to update cart items with new quantities and get warnings
            var warnings = orderedCart.Select(cartItem => new
            {
                ItemId = cartItem.Item.Id,
                Warnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                    cartItem.Item.Id, cartItem.Item.AttributesXml, cartItem.Item.CustomerEnteredPrice,
                    cartItem.Item.RentalStartDateUtc, cartItem.Item.RentalEndDateUtc, cartItem.NewQuantity, true)
            }).ToList();

            //clear entered coupon codes
            _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.DiscountCouponCodeAttribute, null);
            _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.GiftCardCouponCodesAttribute, null);
            
            //Added for clear cache, this is a default nop commerce bug
            _cacheManager.RemoveByPattern(NopCatalogDefaults.ProductPricePatternCacheKey);

            var model = LoadCartData(true);
            var cartString = RenderPartialViewToString("~/Themes/Eshop2021/Views/ShoppingCart/CartDrawer.cshtml", model);

            return Json(new { html = cartString });
        }

        public virtual IActionResult ApplyDiscountCouponAjax(string discountcouponcode)
        {
            //trim
            if (discountcouponcode != null)
                discountcouponcode = discountcouponcode.Trim();

            //cart
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            var model = new ShoppingCartModel();
            if (!string.IsNullOrWhiteSpace(discountcouponcode))
            {
                //we find even hidden records here. this way we can display a user-friendly message if it's expired
                var discounts = _discountService
                    .GetAllDiscountsForCaching(couponCode: discountcouponcode, showHidden: true)
                    .Where(d => d.RequiresCouponCode)
                    .ToList();
                if (discounts.Any())
                {
                    var userErrors = new List<string>();
                    var anyValidDiscount = discounts.Any(discount =>
                    {
                        var validationResult = _discountService.ValidateDiscount(discount, _workContext.CurrentCustomer,
                            new[] { discountcouponcode });
                        userErrors.AddRange(validationResult.Errors);

                        return validationResult.IsValid;
                    });

                    if (anyValidDiscount)
                    {
                        //valid
                        _customerService.ApplyDiscountCouponCode(_workContext.CurrentCustomer, discountcouponcode);
                        model.DiscountBox.Messages.Add(
                            _localizationService.GetResource("ShoppingCart.DiscountCouponCode.Applied"));
                        model.DiscountBox.IsApplied = true;
                    }
                    else
                    {
                        if (userErrors.Any())
                            //some user errors
                            model.DiscountBox.Messages = userErrors;
                        else
                            //general error text
                            model.DiscountBox.Messages.Add(
                                _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                    }
                }
                else
                    //discount cannot be found
                    model.DiscountBox.Messages.Add(
                        _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
            }
            else
                //empty coupon code
                model.DiscountBox.Messages.Add(
                    _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));

            var discountCouponCodes = _customerService.ParseAppliedDiscountCouponCodes(_workContext.CurrentCustomer);
            foreach (var couponCode in discountCouponCodes)
            {
                var discount = _discountService.GetAllDiscountsForCaching(couponCode: couponCode)
                    .FirstOrDefault(d => d.RequiresCouponCode && _discountService.ValidateDiscount(d, _workContext.CurrentCustomer).IsValid);

                if (discount != null)
                {
                    model.DiscountBox.AppliedDiscountsWithCodes.Add(new ShoppingCartModel.DiscountBoxModel.DiscountInfoModel()
                    {
                        Id = discount.Id,
                        CouponCode = discount.CouponCode
                    });
                }
            }
            var discountAppliedWithCodesHtml = RenderPartialViewToString("~/Themes/Eshop2021/Views/Shared/_DiscountAppliedWithCodes.cshtml", model.DiscountBox.AppliedDiscountsWithCodes);
            return Json(new { html = discountAppliedWithCodesHtml });
        }

        [HttpsRequirement(SslRequirement.Yes)]
        public virtual IActionResult Cart()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            var model = new ShoppingCartModel();
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            
            foreach (var item in model.Items)
            {
                var product = _productService.GetProductById(item.ProductId);
                item.IsServiceProductAddedToCart = false;
                if (product.IsService)
                {
                    item.IsServiceProductAddedToCart = true;
                }
            }

            return View(model);
        }

        [HttpPost, PublicAntiForgery]
        public virtual IActionResult AddPackageToCart(int packageId)
        {
            var packageProducts = _packageProductService.GetPackageProductByPackageId(packageId);

            if (packageProducts != null && packageProducts.Count > 0)
            {
                foreach (var item in packageProducts)
                {
                    AddProductToCart_Catalog(item.ProductId, (int)ShoppingCartType.ShoppingCart, 1);
                }
            }

            return View();
        }

        [PublicAntiForgery]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applydiscountcouponcode")]
        public virtual IActionResult ApplyDiscountCoupon(string discountcouponcode, IFormCollection form)
        {
            //trim
            if (discountcouponcode != null)
                discountcouponcode = discountcouponcode.Trim();

            //cart
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            var model = new ShoppingCartModel();
            if (!string.IsNullOrWhiteSpace(discountcouponcode))
            {
                //we find even hidden records here. this way we can display a user-friendly message if it's expired
                var discounts = _discountService
                    .GetAllDiscountsForCaching(couponCode: discountcouponcode, showHidden: true)
                    .Where(d => d.RequiresCouponCode)
                    .ToList();
                if (discounts.Any())
                {
                    var userErrors = new List<string>();
                    var anyValidDiscount = discounts.Any(discount =>
                    {
                        var validationResult = _discountService.ValidateDiscount(discount, _workContext.CurrentCustomer,
                            new[] { discountcouponcode });
                        userErrors.AddRange(validationResult.Errors);

                        return validationResult.IsValid;
                    });

                    if (anyValidDiscount)
                    {
                        //valid
                        _customerService.ApplyDiscountCouponCode(_workContext.CurrentCustomer, discountcouponcode);
                        model.DiscountBox.Messages.Add(
                            _localizationService.GetResource("ShoppingCart.DiscountCouponCode.Applied"));
                        model.DiscountBox.IsApplied = true;
                    }
                    else
                    {
                        if (userErrors.Any())
                            //some user errors
                            model.DiscountBox.Messages = userErrors;
                        else
                            //general error text
                            model.DiscountBox.Messages.Add(
                                _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                    }
                }
                else
                    //discount cannot be found
                    model.DiscountBox.Messages.Add(
                        _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
            }
            else
                //empty coupon code
                model.DiscountBox.Messages.Add(
                    _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));

            //Added for clear cache, this is a default nop commerce bug
            _cacheManager.RemoveByPattern(NopCatalogDefaults.ProductPricePatternCacheKey);
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult GetSimTypeAttributeByDeviceWithPlan(int productId = 0)
        {
            var products = _productService.GetProductById(productId);

            var ProductAttributeOverviewModels = _customeProductModelFactory.PrepareProductAttributeModels(products);

            return Json(new
            {
                html = RenderPartialViewToString("~/Themes/Eshop2021/Views/Product/_SimTypeProductAttributeByDeviceWithPlan.cshtml", ProductAttributeOverviewModels)
            });
        }

        [HttpPost]
        public virtual IActionResult GetProductSimCards(int productId = 0)
        {
            var simcards = _simCardService.GetSimCardListByProductId(productId);

            var simCardList = new List<SelectListItem>();
            if(simcards != null && simcards.Count > 0)
            {
                foreach (var item in simcards)
                {
                    simCardList.Add(new SelectListItem()
                    { 
                        Text = item.CardNumber,
                        Value = Convert.ToString(item.Id)
                    });
                }
            }
            return Json(new { simCardList = simCardList });
        }

        [PublicAntiForgery]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired(FormValueRequirement.StartsWith, "removediscount-")]
        public virtual IActionResult RemoveDiscountCoupon(IFormCollection form)
        {
            var model = new ShoppingCartModel();

            //get discount identifier
            var discountId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("removediscount-", StringComparison.InvariantCultureIgnoreCase))
                    discountId = Convert.ToInt32(formValue.Substring("removediscount-".Length));

            string discountCouponCode = form["hdnDiscountCouponCode-" + discountId];

            if (!string.IsNullOrEmpty(discountCouponCode))
                _customerService.RemoveDiscountCouponCode(_workContext.CurrentCustomer, discountCouponCode);


            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
                
             //Added for clear cache, this is a default nop commerce bug
            _cacheManager.RemoveByPattern(NopCatalogDefaults.ProductPricePatternCacheKey);
            
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        #endregion
    }
}