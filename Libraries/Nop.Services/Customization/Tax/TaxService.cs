using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Tax;

namespace Nop.Services.Tax
{
    public partial class TaxService
    {
        public virtual decimal GetProductPrice(Product product, decimal price,
            int quantity, out decimal taxRate, out decimal taxRate2)
        {
            var customer = _workContext.CurrentCustomer;
            return GetProductPrice(product, price, quantity, customer, out taxRate, out taxRate2);
        }


        public virtual decimal GetProductPrice(Product product, decimal price, int quantity,
            Customer customer, out decimal taxRate, out decimal taxRate2)
        {
            var includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
            return GetProductPrice(product, price, quantity, includingTax, customer, out taxRate, out taxRate2);
        }

        public virtual decimal GetProductPrice(Product product, decimal price, int quantity,
            bool includingTax, Customer customer, out decimal taxRate, out decimal taxRate2)
        {
            var priceIncludesTax = _taxSettings.PricesIncludeTax;
            var taxCategoryId = 0;
            var taxCategory2Id = 0;
            return GetProductPrice(product, taxCategoryId, taxCategory2Id, price, quantity, includingTax, customer,
                priceIncludesTax, out taxRate, out taxRate2);
        }

        public virtual decimal GetProductPrice(Product product, int taxCategoryId, int taxCategory2Id,
            decimal price, int quantity, bool includingTax, Customer customer,
            bool priceIncludesTax, out decimal taxRate, out decimal taxRate2)
        {
            taxRate2 = decimal.Zero;
            //no need to calculate tax rate if passed "price" is 0
            if (price == decimal.Zero)
            {
                taxRate = decimal.Zero;
                return decimal.Zero;
            }

            GetTaxRate(product, taxCategoryId, customer, price, out taxRate, out var isTaxable);

            taxCategory2Id = taxCategory2Id > 0 ? taxCategory2Id : product?.TaxCategory2Id ?? 0;
            if ((product?.IsTaxSplitEnabled ?? false) && taxCategory2Id > 0)
            {
                //Here isTaxable will obviously be the same as first call but we're passing _ to satisfy method args
                GetTaxRate(product, taxCategory2Id, customer, price, out taxRate2, out var _);
            }
            
            //TODo: remove dead code
            /*
            if (priceIncludesTax)
            {
                //"price" already includes tax
                if (includingTax)
                {
                    //we should calculate price WITH tax
                    if (!isTaxable)
                    {
                        //but our request is not taxable
                        //hence we should calculate price WITHOUT tax
                        price = CalculatePrice(price, quantity, taxRate, taxRate2, false, product);
                    }
                }
                else
                {
                    //we should calculate price WITHOUT tax
                    price = CalculatePrice(price, quantity, taxRate, taxRate2, false, product);
                }
            }
            else
            {
                */
                //"price" doesn't include tax
                if (includingTax)
                {
                    //we should calculate price WITH tax
                    //do it only when price is taxable
                    if (isTaxable)
                    {
                        price = CalculatePrice(price, quantity, taxRate, taxRate2, !priceIncludesTax, product);
                    }
                }
//            }

            if (!isTaxable)
            {
                //we return 0% tax rate in case a request is not taxable
                taxRate = decimal.Zero;
                taxRate2 = decimal.Zero;
            }

            return price;
        }

        protected virtual decimal CalculatePrice(decimal price, int quantity,
            decimal percent, decimal percent2, bool increase, Product product)
        {
            if (percent == decimal.Zero && percent2 == decimal.Zero) return price;
            if (!product.IsTaxSplitEnabled) return CalculatePrice(price, percent, increase);

            var split1 = product.SplitAmount;
            var split2 = product.SplitAmount2;

            #region TierPrice

//            var unitPrice = price / quantity;
//            var isTirePrice =
//                quantity > 1 && product.TierPrices.Any(tp =>
//                    tp.Price == unitPrice || tp.Price == price && quantity >= tp.Quantity);
//            if (isTirePrice)
//            {
//                return price;
//            }

            #endregion

            decimal result;
            if (increase)
            {
                var firstPart = split1 + (split1 * percent / 100);
                var secondPart = split2 + (split2 * percent2 / 100);
                result = firstPart + secondPart;
            }
            else
            {
                var firstPart = split1 - (split1 * percent / 100);
                var secondPart = split2 - (split2 * percent2 / 100);
                result = firstPart + secondPart;
                product.SplitAmount = firstPart;
                product.SplitAmount2 = secondPart;
            }

//            if (quantity > 1 && product.Price * quantity == price)
//                result *= quantity;

            return result;
        }
    }
}