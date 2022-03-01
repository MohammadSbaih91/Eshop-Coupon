using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a product attribute value model
    /// </summary>
    public partial class ProductAttributeValueModel 
    {
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.SplitAmount")]
        public decimal SplitAmount { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.SplitAmount2")]
        public decimal SplitAmount2 { get; set; }   
    }
}