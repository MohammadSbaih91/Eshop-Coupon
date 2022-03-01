using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public partial class ProductModel
    {
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.RequireOtherProducts")]
        public bool RequireAnyOneFromOtherProducts { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.RequiredProductIds")]
        public string RequiredAnyOneFromOtherProductIds { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.TaxCategory2")]
        public int TaxCategory2Id { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.TaxSplit")]
        public bool TaxSplit { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.SplitAmount")]
        public decimal SplitAmount { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.SplitAmount2")]
        public decimal SplitAmount2 { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.CustomProductType")]
        public int CustomProductTypeId { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.IsService")]
        public bool IsService { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.IsHidePlanSelection")]
        public bool IsHidePlanSelection { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.OfferDetailsCTA")]
        public string OfferDetailsCTA { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.KnowingTerms")]
        public string KnowingTerms { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Conditions")]
        public string Conditions { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ImportantNotes")]
        public string ImportantNotes { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.DiscountDesc")]
        public string DiscountDesc { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.OneMonthDiscount")]
        public bool OneMonthDiscount { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.SpecialPromotionDesc")]
        public string SpecialPromotionDesc { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.SpecialPromotion")]
        public bool SpecialPromotion { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Fields.IsStudentIdNeeded")]
        public bool IsStudentIdNeeded { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ValidateLocationBasedService")]
        public bool ValidateLocationBasedService { get; set; }
    }

    public partial class ProductLocalizedModel
    {
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.DiscountDesc")]
        public string DiscountDesc { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.SpecialPromotionDesc")]
        public string SpecialPromotionDesc { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.OfferDetailsCTA")]
        public string OfferDetailsCTA { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.KnowingTerms")]
        public string KnowingTerms { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Conditions")]
        public string Conditions { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ImportantNotes")]
        public string ImportantNotes { get; set; }

    }
}