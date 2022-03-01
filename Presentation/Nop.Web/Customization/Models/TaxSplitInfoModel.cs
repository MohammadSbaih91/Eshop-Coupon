using System;
using System.Globalization;

namespace Nop.Web.Models
{
    public partial class TaxSplitInfoModel
    {
        public decimal SplitAmount { get; set; }
        public decimal SplitAmount2 { get; set; }
        public decimal TaxSplit { get; set; }
        public decimal TaxSplit2 { get; set; }
        public bool IsTaxSpitEnable { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal DiscountAmount { get; set; } = decimal.Zero;
        public int MaxDiscountQty { get; set; }
        public decimal Price => SplitAmount + SplitAmount2;
        public bool Is100PercentOff => Price - DiscountAmount <= decimal.Zero && MaxDiscountQty >= Quantity;

        public string SplitInfoFormatted =>
            $"{ToThreeDecimalString(SplitAmount)} ({ToThreeDecimalString(TaxSplit)}%)";

        public string SplitInfoFormatted2 =>
            $"{ToThreeDecimalString(SplitAmount2)} ({ToThreeDecimalString(TaxSplit2)}%)";

        public static string ToThreeDecimalString(decimal value) =>
            (Math.Truncate(value * 1000m) / 1000m).ToString(CultureInfo.InvariantCulture);

        public decimal AmountWithTax
        {
            get
            {
                var part1 = SplitAmount + (SplitAmount * TaxSplit / 100);
                var part2 = SplitAmount2 + (SplitAmount2 * TaxSplit2 / 100);
                var price = part1 + part2;
                var discounted = price - DiscountAmount;
                if (Is100PercentOff || discounted <= decimal.Zero) discounted = decimal.Zero;
                if (Quantity == 1 || MaxDiscountQty <= 0) return discounted * Quantity;

                discounted *= MaxDiscountQty;
                var notDiscounted = price * Math.Abs(Quantity - MaxDiscountQty);
                return discounted + notDiscounted;
            }
        }

        public decimal AmountWithoutTax
        {
            get
            {
                var discounted = Price - DiscountAmount;
                if (discounted <= decimal.Zero) discounted = decimal.Zero;
                if (Quantity == 1 || MaxDiscountQty <= 0) return discounted * Quantity;

                discounted *= MaxDiscountQty;
                var notDiscounted = Price * Math.Abs(Quantity - MaxDiscountQty);
                return discounted + notDiscounted;
            }
        }

        public decimal TaxAmount
        {
            get
            {
                if (Is100PercentOff) return decimal.Zero;

                var part1 = SplitAmount + (SplitAmount * TaxSplit / 100);
                var part2 = SplitAmount2 + (SplitAmount2 * TaxSplit2 / 100);
                return (part1 + part2) * Quantity - (SplitAmount + SplitAmount2) * Quantity;
            }
        }
    }
}