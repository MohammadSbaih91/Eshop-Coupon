using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Discounts;

namespace Nop.Services.Customization.Discounts
{
    public class CustomDiscountService : ICustomDiscountService
    {
        #region Fields
        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DiscountCouponCodes> _discountCouponCodesRepository;
        #endregion

        #region Ctor
        public CustomDiscountService(IRepository<Discount> discountRepository,
            IRepository<DiscountCouponCodes> discountCouponCodesRepository)
        {
            _discountRepository = discountRepository;
            _discountCouponCodesRepository = discountCouponCodesRepository;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Check discount coupon code is exist in entire system
        /// </summary>
        /// <param name="discountCouponCode">coupon</param>
        /// <param name="discountid">discount identity</param>
        /// <returns></returns>
        public bool IsDiscountCouponCodeExist(string discountCouponCode, int discountid = 0)
        {
            var query = _discountRepository.Table;

            if (discountid != 0)
            {
                query = query.Where(p => p.Id != discountid && p.CouponCode == discountCouponCode);
            }
            else
            {
                query = query.Where(p => p.CouponCode == discountCouponCode);
            }

            if (query != null && query.Count() > 0)
                return true;

            return false;
        }
        #endregion

        #region Dicount Coupon Code
        public IList<DiscountCouponCodes> GetDiscountCouponCodesList(int discountId)
        {
            var result = from dc in _discountCouponCodesRepository.Table
                         where dc.DiscountId == discountId
                         select dc;
            return result.ToList();
        }

        public DiscountCouponCodes GetDiscountCouponCodeById(int Id)
        {
            if (Id == 0)
                return null;

            return _discountCouponCodesRepository.GetById(Id);
        }
        public void InsertDiscountCouponCode(DiscountCouponCodes discountCouponCodes)
        {
            _discountCouponCodesRepository.Insert(discountCouponCodes);
        }

        public void DeleteDiscountCouponCode(DiscountCouponCodes discountCouponCodes)
        {
            _discountCouponCodesRepository.Delete(discountCouponCodes);
        }

        public bool CouponCodeIsExist(string couponCode)
        {
            var result = (from dc in _discountCouponCodesRepository.Table
                         where dc.CouponCode == couponCode
                         select dc);

            return result.Any();   
        }

        public DiscountCouponCodes GetCouponCodeByDiscountCode(string couponCode)
        {
            return _discountCouponCodesRepository.Table.FirstOrDefault(p => p.CouponCode == couponCode);
        }

        public void UpdateDiscountUsedCount(string couponCode)
        {
            var coupon = GetCouponCodeByDiscountCode(couponCode);
            if (coupon != null)
            {
                coupon.IsUsedCount++;
                _discountCouponCodesRepository.Update(coupon);
            }
        }
        #endregion
    }
}
