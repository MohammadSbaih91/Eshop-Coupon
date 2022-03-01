using Nop.Core.Domain.Enum;
namespace Nop.Web.Models.Catalog
{
    public partial class ProductDetailsModel 
    {
        public int PackageId { get; set; }

        public bool IsHidePlanSelection { get; set; }

        public string OfferDetailsCTA { get; set; }

        public string KnowingTerms { get; set; }

        public string Conditions { get; set; }

        public string ImportantNotes { get; set; }

        public partial class ProductPriceModel 
        {
            public EnumProductDetail enumProductDetail { get; set; }
            
            public TaxSplitInfoModel TaxSplitInfo { get; set; } = new TaxSplitInfoModel();
            public string PriceWithOutTax { get; set; }
        }    

        public partial class ProductAttributeModel
        {
            public bool IsShowButton { get; set; }
        }
        public partial class AddToCartModel
        {
            public bool ValidateLocationBasedService { get; set; }
            public bool OutOfStock { get; set; }
        }
    }
}