using Nop.Core.Domain.Discounts;
using System.Collections.Generic;

namespace Nop.Services.Customization.Discounts
{
    public interface ICustomDiscountService
    {
        /// <summary>
        /// Check discount coupon code is exist in entire system
        /// </summary>
        /// <param name="discountCouponCode">coupon</param>
        /// <param name="discountid">discount identity</param>
        /// <returns></returns>
        bool IsDiscountCouponCodeExist(string discountCouponCode, int discountid = 0);

        #region Dicount Coupon Code
        IList<DiscountCouponCodes> GetDiscountCouponCodesList(int discountId);
        DiscountCouponCodes GetDiscountCouponCodeById(int Id);
        void InsertDiscountCouponCode(DiscountCouponCodes discountCouponCodes);
        void DeleteDiscountCouponCode(DiscountCouponCodes discountCouponCodes);
        bool CouponCodeIsExist(string couponCode);
        DiscountCouponCodes GetCouponCodeByDiscountCode(string couponCode);

        void UpdateDiscountUsedCount(string couponCode);
        #endregion
    }
}
