using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Events;
using Nop.Services.Security;

namespace Nop.Services.Orders
{
    public partial class ShoppingCartService
    {
        #region Utilities
        public virtual bool AnyOneItemInCart(int quantity, int shoppingCartItemId, int requiredQuantity,
           IReadOnlyCollection<ShoppingCartItem> cart, IEnumerable<Product> requiredProducts)
        {
            foreach (var requiredProduct in requiredProducts)
            {
                var requiredProductRequiredQuantity = RequiredProductRequiredQuantity(quantity, shoppingCartItemId,
                    requiredQuantity, cart, requiredProduct.Id);
                var anyOneItemInCart = requiredProductRequiredQuantity -
                                       (cart.FirstOrDefault(item => item.ProductId == requiredProduct.Id)?.Quantity ??
                                        0) <= 0;
                if (anyOneItemInCart) return true;
            }

            return false;
        }

        public int RequiredProductRequiredQuantity(int quantity, int shoppingCartItemId, int requiredProductQuantity,
            IEnumerable<ShoppingCartItem> cart, int requiredProductId)
        {
            return quantity * (requiredProductQuantity + cart
                                   .Where(item =>
                                       item.Product.RequireAnyOneFromOtherProducts &&
                                       _productService.ParseRequiredAnyOneFromOtherProductIds(item.Product)
                                           .Contains(requiredProductId))
                                   .Where(item => item.Id != shoppingCartItemId)
                                   .Sum(item => item.Quantity * requiredProductQuantity));
        }
        #endregion

        /// <summary>
        /// Validates shopping cart item
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="product">Product</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">Customer entered price</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="addRequiredProducts">Whether to add required products</param>
        /// <param name="shoppingCartItemId">Shopping cart identifier; pass 0 if it's a new item</param>
        /// <param name="getStandardWarnings">A value indicating whether we should validate a product for standard properties</param>
        /// <param name="getAttributesWarnings">A value indicating whether we should validate product attributes</param>
        /// <param name="getGiftCardWarnings">A value indicating whether we should validate gift card properties</param>
        /// <param name="getRequiredProductWarnings">A value indicating whether we should validate required products (products which require other products to be added to the cart)</param>
        /// <param name="getRentalWarnings">A value indicating whether we should validate rental properties</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> GetShoppingCartItemWarnings(Customer customer, ShoppingCartType shoppingCartType,
            Product product, int storeId,
            string attributesXml, decimal customerEnteredPrice,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool addRequiredProducts = true, int shoppingCartItemId = 0,
            bool getStandardWarnings = true, bool getAttributesWarnings = true,
            bool getGiftCardWarnings = true, bool getRequiredProductWarnings = true,
            bool getRentalWarnings = true, bool isAddtoCart = false, int selectedProductId = 0)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();

            //standard properties
            if (getStandardWarnings)
                warnings.AddRange(GetStandardWarnings(customer, shoppingCartType, product, attributesXml, customerEnteredPrice, quantity));

            //selected attributes
            if (getAttributesWarnings)
                warnings.AddRange(GetShoppingCartItemAttributeWarnings(customer, shoppingCartType, product, quantity, attributesXml));

            //gift cards
            if (getGiftCardWarnings)
                warnings.AddRange(GetShoppingCartItemGiftCardWarnings(shoppingCartType, product, attributesXml));

            //required products
            if (getRequiredProductWarnings)
                warnings.AddRange(GetRequiredProductWarnings(customer, shoppingCartType, product, storeId, quantity, addRequiredProducts, shoppingCartItemId));

            //required products any one from other
            if (!isAddtoCart)
                warnings.AddRange(GetRequireAnyOneProductFromOtherWarnings(customer, shoppingCartType, product, storeId, quantity, addRequiredProducts, shoppingCartItemId, selectedProductId));

            //rental products
            if (getRentalWarnings)
                warnings.AddRange(GetRentalProductWarnings(product, rentalStartDate, rentalEndDate));

            return warnings;
        }

        /// <summary>
        /// Validates required products (products which require some other products to be added to the cart)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="product">Product</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="addRequiredProducts">Whether to add required products</param>
        /// <param name="shoppingCartItemId">Shopping cart identifier; pass 0 if it's a new item</param>
        /// <returns>Warnings</returns>
        public IList<string> GetRequireAnyOneProductFromOtherWarnings(Customer customer, ShoppingCartType shoppingCartType,
            Product product,
            int storeId, int quantity, bool addRequiredProducts, int shoppingCartItemId, int selectedProductId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();

            //Add condition here
            var devicePackage = customer.ShoppingCartItems.Where(x => x.Id == shoppingCartItemId).Select(x => x.DevicePackage).FirstOrDefault();
            if (devicePackage == "DeviceOnly") return warnings;

            if (CheckRequiredProductInMainProduct(product, selectedProductId))
                return warnings;

            //var warnings = base.GetRequiredProductWarnings(customer, shoppingCartType, product, storeId, quantity,
            //    addRequiredProducts, shoppingCartItemId);

                //at now we ignore quantities of required products and use 1
            var requiredProductQuantity = 1;

                //get customer shopping cart
                var cart = customer.ShoppingCartItems
                    .Where(item => item.ShoppingCartType == shoppingCartType)
                    .LimitPerStore(storeId).ToList();

                //whether other cart items require the passed product
                var passedProductRequiredQuantity = cart.Where(item =>
                {
                    if (!item.Product.RequireAnyOneFromOtherProducts) return false;
                    var requiredProductIds = _productService.ParseRequiredAnyOneFromOtherProductIds(item.Product);
                    return requiredProductIds.Contains(product.Id)
                           && !cart.Any(i => requiredProductIds.Where(id => id != product.Id).Contains(i.Product.Id));
                }).Sum(item => item.Quantity * requiredProductQuantity);

                if (passedProductRequiredQuantity > quantity)
                    warnings.Add(string.Format(
                        _localizationService.GetResource("ShoppingCart.RequiredProductUpdateWarning"),
                        passedProductRequiredQuantity));

                //whether the passed product requires other products
                if (!product.RequireAnyOneFromOtherProducts)
                    return warnings;

                //get these required products
                var requiredProducts =
                    _productService.GetProductsByIds(_productService.ParseRequiredAnyOneFromOtherProductIds(product));
                if (!requiredProducts.Any())
                    return warnings;

                var anyOneItemInCart = AnyOneItemInCart(quantity, shoppingCartItemId,
                    requiredProductQuantity, cart, requiredProducts);

                if (anyOneItemInCart) return warnings;

                //get warnings
                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                var warningLocale = _localizationService.GetResource("ShoppingCart.RequiredAnyOneFromOtherProductWarning");
                var warningList = new List<string>();
                if (product.RequireOtherProducts)
                    requiredProducts = requiredProducts
                        .Where(p => !_productService.ParseRequiredProductIds(product).Contains(p.Id)).ToList();

                foreach (var requiredProduct in requiredProducts)
                {
                    var requiredProductRequiredQuantity = RequiredProductRequiredQuantity(quantity, shoppingCartItemId,
                        requiredProductQuantity, cart, requiredProduct.Id);
                    var requiredProductName =
                        WebUtility.HtmlEncode(_localizationService.GetLocalized(requiredProduct, x => x.Name));

                    warningList.Add(_catalogSettings.UseLinksInRequiredProductWarnings
                        ? $"<a href=\"{urlHelper.RouteUrl(nameof(Product), new { SeName = _urlRecordService.GetSeName(requiredProduct) })}\">{requiredProductName} ({requiredProductRequiredQuantity})</a>"
                        : $"{requiredProductName} ({requiredProductRequiredQuantity})");
                }

                warnings.Add(string.Format(warningLocale, string.Join(", ", warningList)));
                return warnings;
        }

        /// <summary>
        /// Add a product to shopping cart
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="product">Product</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">The price enter by a customer</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="addRequiredProducts">Whether to add required products</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> AddToCart(Customer customer, Product product,
            ShoppingCartType shoppingCartType, int storeId, string attributesXml = null,
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool addRequiredProducts = true, decimal subcidyDiscount = 0, int simCardId = 0)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();
            if (shoppingCartType == ShoppingCartType.ShoppingCart && !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, customer))
            {
                warnings.Add("Shopping cart is disabled");
                return warnings;
            }

            if (shoppingCartType == ShoppingCartType.Wishlist && !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist, customer))
            {
                warnings.Add("Wishlist is disabled");
                return warnings;
            }

            if (customer.IsSearchEngineAccount())
            {
                warnings.Add("Search engine can't add to cart");
                return warnings;
            }

            if (quantity <= 0)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));
                return warnings;
            }

            //reset checkout info
            _customerService.ResetCheckoutData(customer, storeId);

            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == shoppingCartType && sci.PackageId == 0)
                .LimitPerStore(storeId)
                .ToList();

            var shoppingCartItem = FindShoppingCartItemInTheCart(cart,
                    shoppingCartType, product, attributesXml, customerEnteredPrice,
                    rentalStartDate, rentalEndDate);

            //var shoppingCartItem = new ShoppingCartItem();
            //if (!isAddRequiredProduct)
            //{
            //    shoppingCartItem = FindShoppingCartItemInTheCart(cart,
            //        shoppingCartType, product, attributesXml, customerEnteredPrice,
            //        rentalStartDate, rentalEndDate);
            //}
            //else
            //{
            //    shoppingCartItem = null;
            //}

            if (shoppingCartItem != null)
            {
                //update existing shopping cart item
                var newQuantity = shoppingCartItem.Quantity + quantity;
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, product,
                    storeId, attributesXml,
                    customerEnteredPrice, rentalStartDate, rentalEndDate,
                    newQuantity, addRequiredProducts, shoppingCartItem.Id, isAddtoCart: true));

                if (warnings.Any())
                    return warnings;

                shoppingCartItem.AttributesXml = attributesXml;
                shoppingCartItem.Quantity = newQuantity;
                shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                shoppingCartItem.SubsidyDiscount = (shoppingCartItem.SubsidyDiscount + subcidyDiscount);
                shoppingCartItem.SimCardId = simCardId;
                _customerService.UpdateCustomer(customer);

                //event notification
                _eventPublisher.EntityUpdated(shoppingCartItem);
            }
            else
            {
                //new shopping cart item
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, product,
                    storeId, attributesXml, customerEnteredPrice,
                    rentalStartDate, rentalEndDate,
                    quantity, addRequiredProducts, isAddtoCart: true));

                if (warnings.Any())
                    return warnings;

                //maximum items validation
                switch (shoppingCartType)
                {
                    case ShoppingCartType.ShoppingCart:
                        if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                        {
                            warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumShoppingCartItems"), _shoppingCartSettings.MaximumShoppingCartItems));
                            return warnings;
                        }

                        break;
                    case ShoppingCartType.Wishlist:
                        if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                        {
                            warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumWishlistItems"), _shoppingCartSettings.MaximumWishlistItems));
                            return warnings;
                        }

                        break;
                    default:
                        break;
                }

                var now = DateTime.UtcNow;
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartType = shoppingCartType,
                    StoreId = storeId,
                    Product = product,
                    AttributesXml = attributesXml,
                    CustomerEnteredPrice = customerEnteredPrice,
                    Quantity = quantity,
                    RentalStartDateUtc = rentalStartDate,
                    RentalEndDateUtc = rentalEndDate,
                    CreatedOnUtc = now,
                    UpdatedOnUtc = now,
                    SubsidyDiscount = subcidyDiscount,
                    SimCardId = simCardId
                };
                customer.ShoppingCartItems.Add(shoppingCartItem);
                _customerService.UpdateCustomer(customer);

                //updated "HasShoppingCartItems" property used for performance optimization
                customer.HasShoppingCartItems = customer.ShoppingCartItems.Any();
                _customerService.UpdateCustomer(customer);

                //event notification
                _eventPublisher.EntityInserted(shoppingCartItem);
            }

            return warnings;
        }


        /// <summary>
        /// Migrate shopping cart
        /// </summary>
        /// <param name="fromCustomer">From customer</param>
        /// <param name="toCustomer">To customer</param>
        /// <param name="includeCouponCodes">A value indicating whether to coupon codes (discount and gift card) should be also re-applied</param>
        public virtual void MigrateShoppingCart(Customer fromCustomer, Customer toCustomer, bool includeCouponCodes)
        {
            if (fromCustomer == null)
                throw new ArgumentNullException(nameof(fromCustomer));
            if (toCustomer == null)
                throw new ArgumentNullException(nameof(toCustomer));

            if (fromCustomer.Id == toCustomer.Id)
                return; //the same customer

            //shopping cart items
            var fromCart = fromCustomer.ShoppingCartItems.ToList();
            for (var i = 0; i < fromCart.Count; i++)
            {
                var sci = fromCart[i];
                AddToCart(toCustomer, sci.Product, sci.ShoppingCartType, sci.StoreId,
                    sci.AttributesXml, sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, false,sci.SubsidyDiscount,sci.SimCardId);
            }

            for (var i = 0; i < fromCart.Count; i++)
            {
                var sci = fromCart[i];
                DeleteShoppingCartItem(sci);
            }

            //copy discount and gift card coupon codes
            if (includeCouponCodes)
            {
                //discount
                foreach (var code in _customerService.ParseAppliedDiscountCouponCodes(fromCustomer))
                    _customerService.ApplyDiscountCouponCode(toCustomer, code);

                //gift card
                foreach (var code in _customerService.ParseAppliedGiftCardCouponCodes(fromCustomer))
                    _customerService.ApplyGiftCardCouponCode(toCustomer, code);

                //save customer
                _customerService.UpdateCustomer(toCustomer);
            }

            //move selected checkout attributes
            var checkoutAttributesXml = _genericAttributeService.GetAttribute<string>(fromCustomer, NopCustomerDefaults.CheckoutAttributes, _storeContext.CurrentStore.Id);
            _genericAttributeService.SaveAttribute(toCustomer, NopCustomerDefaults.CheckoutAttributes, checkoutAttributesXml, _storeContext.CurrentStore.Id);
        }

        /// <summary>
        /// Add a product to shopping cart
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="product">Product</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">The price enter by a customer</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="addRequiredProducts">Whether to add required products</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> AddToCartWithPackage(int packageId,Customer customer, Product product,
            ShoppingCartType shoppingCartType, int storeId, string attributesXml = null,
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool addRequiredProducts = true)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();
            if (shoppingCartType == ShoppingCartType.ShoppingCart && !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, customer))
            {
                warnings.Add("Shopping cart is disabled");
                return warnings;
            }

            if (shoppingCartType == ShoppingCartType.Wishlist && !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist, customer))
            {
                warnings.Add("Wishlist is disabled");
                return warnings;
            }

            if (customer.IsSearchEngineAccount())
            {
                warnings.Add("Search engine can't add to cart");
                return warnings;
            }

            if (quantity <= 0)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));
                return warnings;
            }

            //reset checkout info
            _customerService.ResetCheckoutData(customer, storeId);

            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == shoppingCartType)
                .LimitPerStore(storeId)
                .ToList();

            var shoppingCartItem = FindShoppingCartItemInTheCart(cart,
                    shoppingCartType, product, attributesXml, customerEnteredPrice,
                    rentalStartDate, rentalEndDate);

            if (shoppingCartItem != null && shoppingCartItem.PackageId == packageId)
            {
                //update existing shopping cart item
                var newQuantity = shoppingCartItem.Quantity + quantity;
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, product,
                    storeId, attributesXml,
                    customerEnteredPrice, rentalStartDate, rentalEndDate,
                    newQuantity, addRequiredProducts, shoppingCartItem.Id, isAddtoCart: true));

                if (warnings.Any())
                    return warnings;

                shoppingCartItem.AttributesXml = attributesXml;
                shoppingCartItem.Quantity = newQuantity;
                shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                _customerService.UpdateCustomer(customer);

                //event notification
                _eventPublisher.EntityUpdated(shoppingCartItem);
            }
            else
            {
                //new shopping cart item
                warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartType, product,
                    storeId, attributesXml, customerEnteredPrice,
                    rentalStartDate, rentalEndDate,
                    quantity, addRequiredProducts, isAddtoCart: true));

                if (warnings.Any())
                    return warnings;

                //maximum items validation
                switch (shoppingCartType)
                {
                    case ShoppingCartType.ShoppingCart:
                        if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                        {
                            warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumShoppingCartItems"), _shoppingCartSettings.MaximumShoppingCartItems));
                            return warnings;
                        }

                        break;
                    case ShoppingCartType.Wishlist:
                        if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                        {
                            warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumWishlistItems"), _shoppingCartSettings.MaximumWishlistItems));
                            return warnings;
                        }

                        break;
                    default:
                        break;
                }

                var now = DateTime.UtcNow;
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartType = shoppingCartType,
                    StoreId = storeId,
                    Product = product,
                    AttributesXml = attributesXml,
                    CustomerEnteredPrice = customerEnteredPrice,
                    Quantity = quantity,
                    RentalStartDateUtc = rentalStartDate,
                    RentalEndDateUtc = rentalEndDate,
                    CreatedOnUtc = now,
                    UpdatedOnUtc = now,
                    PackageId = packageId
                };
                customer.ShoppingCartItems.Add(shoppingCartItem);
                _customerService.UpdateCustomer(customer);

                //updated "HasShoppingCartItems" property used for performance optimization
                customer.HasShoppingCartItems = customer.ShoppingCartItems.Any();
                _customerService.UpdateCustomer(customer);

                //event notification
                _eventPublisher.EntityInserted(shoppingCartItem);
            }

            return warnings;
        }

        public virtual IList<string> GetRequiredProductWarnings(Customer customer, ShoppingCartType shoppingCartType, Product product,
            int storeId, int quantity, bool addRequiredProducts, int shoppingCartItemId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();

            //at now we ignore quantities of required products and use 1
            var requiredProductQuantity = 1;

            //get customer shopping cart
            var cart = customer.ShoppingCartItems
                .Where(item => item.ShoppingCartType == shoppingCartType)
                .LimitPerStore(storeId).ToList();

            //whether other cart items require the passed product
            var passedProductRequiredQuantity = cart
                .Where(item => item.Product.RequireOtherProducts && _productService.ParseRequiredProductIds(item.Product).Contains(product.Id))
                .Sum(item => item.Quantity * requiredProductQuantity);
            if (passedProductRequiredQuantity > quantity)
                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.RequiredProductUpdateWarning"), passedProductRequiredQuantity));

            //whether the passed product requires other products
            if (!product.RequireOtherProducts)
                return warnings;

            //get these required products
            var requiredProducts = _productService.GetProductsByIds(_productService.ParseRequiredProductIds(product));
            if (!requiredProducts.Any())
                return warnings;

            //get warnings
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var warningLocale = _localizationService.GetResource("ShoppingCart.RequiredProductWarning");
            foreach (var requiredProduct in requiredProducts)
            {
                //get the required quantity of the required product
                var requiredProductRequiredQuantity = quantity * requiredProductQuantity + cart
                    .Where(item => item.Product.RequireOtherProducts && _productService.ParseRequiredProductIds(item.Product).Contains(requiredProduct.Id))
                    .Where(item => item.Id != shoppingCartItemId)
                    .Sum(item => item.Quantity * requiredProductQuantity);

                //whether required product is already in the cart in the required quantity
                var quantityToAdd = requiredProductRequiredQuantity - (cart.FirstOrDefault(item => item.ProductId == requiredProduct.Id)?.Quantity ?? 0);
                if (quantityToAdd <= 0)
                    continue;

                //prepare warning message
                var requiredProductName = WebUtility.HtmlEncode(_localizationService.GetLocalized(requiredProduct, x => x.Name));
                var requiredProductWarning = _catalogSettings.UseLinksInRequiredProductWarnings
                    ? string.Format(warningLocale, $"<a href=\"{urlHelper.RouteUrl(nameof(Product), new { SeName = _urlRecordService.GetSeName(requiredProduct) })}\">{requiredProductName}</a>", requiredProductRequiredQuantity)
                    : string.Format(warningLocale, requiredProductName, requiredProductRequiredQuantity);

                //add to cart (if possible)
                if (addRequiredProducts && product.AutomaticallyAddRequiredProducts)
                {
                    //do not add required products to prevent circular references
                    var addToCartWarnings = AddToCart(customer, requiredProduct, shoppingCartType, storeId,
                        quantity: quantityToAdd, addRequiredProducts: false);

                    //don't display all specific errors only the generic one
                    if (addToCartWarnings.Any())
                        warnings.Add(requiredProductWarning);
                }
                else
                    warnings.Add(requiredProductWarning);
            }

            return warnings;
        }

        public virtual IList<string> UpdateShoppingCart(Customer customer, int shoppingCartItemId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var warnings = new List<string>();
            var shoppingCartItem = customer.ShoppingCartItems.FirstOrDefault(sci => sci.Id == shoppingCartItemId);

            if (shoppingCartItem == null)
                return warnings;

            shoppingCartItem.PackageId = 0;
            shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
            _customerService.UpdateCustomer(customer);

            return warnings;
        }

        /// <summary>
        /// Check if the product is added to cart required any one is from main product
        /// </summary>
        /// <param name="mainProductId">Product Detail page product Id</param>
        /// <param name="selectedProductId">Plan or Device product id based on selected on product detail page</param>
        /// <returns></returns>
        public virtual bool CheckRequiredProductInMainProduct(Product mainProduct,int selectedProductId)
        {
            var requiredProductIds = _productService.ParseRequiredAnyOneFromOtherProductIds(mainProduct);
            return requiredProductIds.Contains(selectedProductId);
        }
    }
}
