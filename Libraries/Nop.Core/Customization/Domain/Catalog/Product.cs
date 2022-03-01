namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product
    /// </summary>
    public partial class Product
    {
         
        public bool RequireAnyOneFromOtherProducts { get; set; }
        
        public string RequiredAnyOneFromOtherProductIds { get; set; }
        
        public bool TaxSplit { get; set; }

        public int TaxCategory2Id { get; set; }

        public decimal SplitAmount { get; set; }

        public decimal SplitAmount2 { get; set; }

        public bool IsTaxSplitEnabled => !IsTaxExempt && TaxSplit && (TaxCategory2Id > decimal.Zero && SplitAmount2 > 0
                                                                      || TaxCategoryId > decimal.Zero &&
                                                                      SplitAmount > 0);

        public int CustomProductTypeId { get; set; }
        public bool IsService { get; set; }

        public bool IsHidePlanSelection { get; set; }

        public string OfferDetailsCTA { get; set; }

        public string KnowingTerms { get; set; }

        public string Conditions { get; set; }

        public string ImportantNotes { get; set; }

        public bool ValidateLocationBasedService { get; set; }

        public bool IsStudentIdNeeded { get; set; }

        public bool OneMonthDiscount { get; set; }        

        public string DiscountDesc { get; set; }

        public bool SpecialPromotion { get; set; }

        public string SpecialPromotionDesc { get; set; }
    }
}