using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Customization.Discounts;

namespace Nop.Services.Discounts
{
    public partial class DiscountService
    {
        #region Fields
        public readonly ICustomDiscountService _customDiscountService;
        public readonly IRepository<DiscountCouponCodes> _discountCouponCodesRepository;
        #endregion

        #region Ctor

        public DiscountService(ICategoryService categoryService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService,
            IPluginFinder pluginFinder,
            IRepository<Category> categoryRepository,
            IRepository<Discount> discountRepository,
            IRepository<DiscountRequirement> discountRequirementRepository,
            IRepository<DiscountUsageHistory> discountUsageHistoryRepository,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Product> productRepository,
            IStaticCacheManager cacheManager,
            IStoreContext storeContext,
            ICustomDiscountService customDiscountService,
            IRepository<DiscountCouponCodes> discountCouponCodesRepository)
        {
            this._categoryService = categoryService;
            this._customerService = customerService;
            this._eventPublisher = eventPublisher;
            this._localizationService = localizationService;
            this._pluginFinder = pluginFinder;
            this._categoryRepository = categoryRepository;
            this._discountRepository = discountRepository;
            this._discountRequirementRepository = discountRequirementRepository;
            this._discountUsageHistoryRepository = discountUsageHistoryRepository;
            this._manufacturerRepository = manufacturerRepository;
            this._productRepository = productRepository;
            this._cacheManager = cacheManager;
            this._storeContext = storeContext;
            this._customDiscountService = customDiscountService;
            this._discountCouponCodesRepository = discountCouponCodesRepository;
        }

        #endregion

        #region Utilities
        protected bool AnyCouponCodeMatch(string[] couponCodesToValidate, int discountId)
        {
            var discountCouponCodes = (from dcc in _discountCouponCodesRepository.Table
                                       where dcc.DiscountId == discountId
                                       select dcc).ToList();

            var result = discountCouponCodes.Where(x => couponCodesToValidate.Contains(x.CouponCode)).Count();
            if (result > 0)
                return true;
            else
                return false;
        }
        #endregion

        /// <summary>
        /// Gets all discounts
        /// </summary>
        /// <param name="discountType">Discount type; pass null to load all records</param>
        /// <param name="couponCode">Coupon code to find (exact match); pass null or empty to load all records</param>
        /// <param name="discountName">Discount name; pass null or empty to load all records</param>
        /// <param name="showHidden">A value indicating whether to show expired and not started discounts</param>
        /// <param name="startDateUtc">Discount start date; pass null to load all records</param>
        /// <param name="endDateUtc">Discount end date; pass null to load all records</param>
        /// <returns>Discounts</returns>
        public virtual IList<Discount> GetAllDiscounts(DiscountType? discountType = null,
            string couponCode = null, string discountName = null, bool showHidden = false,
            DateTime? startDateUtc = null, DateTime? endDateUtc = null)
        {
            var _discountCouponCodes = EngineContext.Current.Resolve<IRepository<DiscountCouponCodes>>();
            var query = _discountRepository.Table;

            if (!showHidden)
            {
                query = query.Where(discount =>
                    (!discount.StartDateUtc.HasValue || discount.StartDateUtc <= DateTime.UtcNow) &&
                    (!discount.EndDateUtc.HasValue || discount.EndDateUtc >= DateTime.UtcNow));
            }

            //filter by dates
            if (startDateUtc.HasValue)
                query = query.Where(discount => !discount.StartDateUtc.HasValue || discount.StartDateUtc >= startDateUtc.Value);
            if (endDateUtc.HasValue)
                query = query.Where(discount => !discount.EndDateUtc.HasValue || discount.EndDateUtc <= endDateUtc.Value);

            //filter by coupon code
            if (!string.IsNullOrEmpty(couponCode))
            {
                query = from dc in query
                        join dcc in _discountCouponCodes.Table on dc.Id equals dcc.DiscountId
                        where dcc.CouponCode == couponCode
                        select dc;

                //query = query.Where(discount => discount.CouponCode == couponCode);
            }

            //filter by name
            if (!string.IsNullOrEmpty(discountName))
                query = query.Where(discount => discount.Name.Contains(discountName));

            //filter by type
            if (discountType.HasValue)
                query = query.Where(discount => discount.DiscountTypeId == (int)discountType.Value);

            query = query.OrderBy(discount => discount.Name).ThenBy(discount => discount.Id);

            return query.ToList();
        }

        /// <summary>
        /// Validate discount
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <param name="couponCodesToValidate">Coupon codes to validate</param>
        /// <returns>Discount validation result</returns>
        public virtual DiscountValidationResult ValidateDiscount(DiscountForCaching discount, Customer customer, string[] couponCodesToValidate)
        {
            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //invalid by default
            var result = new DiscountValidationResult();

            //check coupon code
            if (discount.RequiresCouponCode)
            {
                //if (string.IsNullOrEmpty(discount.CouponCode))
                //    return result;

                //if (couponCodesToValidate == null)
                //    return result;

                //if (!couponCodesToValidate.Any(x => x.Equals(discount.CouponCode, StringComparison.InvariantCultureIgnoreCase)))
                //    return result;

                if (!AnyCouponCodeMatch(couponCodesToValidate, discount.Id))
                    return result;
            }

            //Do not allow discounts applied to order subtotal or total when a customer has gift cards in the cart.
            //Otherwise, this customer can purchase gift cards with discount and get more than paid ("free money").
            if (discount.DiscountType == DiscountType.AssignedToOrderSubTotal ||
                discount.DiscountType == DiscountType.AssignedToOrderTotal)
            {
                var cart = customer.ShoppingCartItems
                    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_storeContext.CurrentStore.Id)
                    .ToList();

                var hasGiftCards = cart.Any(x => x.Product.IsGiftCard);
                if (hasGiftCards)
                {
                    result.Errors = new List<string> { _localizationService.GetResource("ShoppingCart.Discount.CannotBeUsedWithGiftCards") };
                    return result;
                }
            }

            //check date range
            var now = DateTime.UtcNow;
            if (discount.StartDateUtc.HasValue)
            {
                var startDate = DateTime.SpecifyKind(discount.StartDateUtc.Value, DateTimeKind.Utc);
                if (startDate.CompareTo(now) > 0)
                {
                    result.Errors = new List<string> { _localizationService.GetResource("ShoppingCart.Discount.NotStartedYet") };
                    return result;
                }
            }

            if (discount.EndDateUtc.HasValue)
            {
                var endDate = DateTime.SpecifyKind(discount.EndDateUtc.Value, DateTimeKind.Utc);
                if (endDate.CompareTo(now) < 0)
                {
                    result.Errors = new List<string> { _localizationService.GetResource("ShoppingCart.Discount.Expired") };
                    return result;
                }
            }

            //discount limitation
            switch (discount.DiscountLimitation)
            {
                case DiscountLimitationType.NTimesOnly:
                    {
                        if (discount.RequiresCouponCode)
                        {
                            foreach (var couponCode in couponCodesToValidate)
                            {
                                var couponCodes = _customDiscountService.GetCouponCodeByDiscountCode(couponCode);
                                if (couponCodes.IsUsedCount >= discount.LimitationTimes)
                                    return result;
                            }
                        }
                        else
                        {
                            var usedTimes = GetAllDiscountUsageHistory(discount.Id, null, null, 0, 1).TotalCount;
                            if (usedTimes >= discount.LimitationTimes)
                                return result;
                        }
                    }

                    break;
                //case DiscountLimitationType.NTimesPerCustomer:
                //    {
                //        if (customer.IsRegistered())
                //        {
                //            var usedTimes = GetAllDiscountUsageHistory(discount.Id, customer.Id, null, 0, 1).TotalCount;
                //            if (usedTimes >= discount.LimitationTimes)
                //            {
                //                result.Errors = new List<string> { _localizationService.GetResource("ShoppingCart.Discount.CannotBeUsedAnymore") };
                //                return result;
                //            }
                //        }
                //    }

                //    break;
                case DiscountLimitationType.Unlimited:
                default:
                    break;
            }

            //discount requirements
            var key = string.Format(NopDiscountDefaults.DiscountRequirementModelCacheKey, discount.Id);
            var requirementsForCaching = _cacheManager.Get(key, () =>
            {
                var requirements = GetAllDiscountRequirements(discount.Id, true);
                return GetReqirementsForCaching(requirements);
            });

            //get top-level group
            var topLevelGroup = requirementsForCaching.FirstOrDefault();
            if (topLevelGroup == null || (topLevelGroup.IsGroup && !topLevelGroup.ChildRequirements.Any()) || !topLevelGroup.InteractionType.HasValue)
            {
                //there are no requirements, so discount is valid
                result.IsValid = true;
                return result;
            }

            //requirements exist, let's check them
            var errors = new List<string>();
            result.IsValid = GetValidationResult(requirementsForCaching, topLevelGroup.InteractionType.Value, customer, errors);

            //set errors if result is not valid
            if (!result.IsValid)
                result.Errors = errors;

            return result;
        }
    }
}
