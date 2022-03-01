using Nop.Core.Domain.Catalog;

namespace Nop.Services.Customization.Catalog
{
    /// <summary>
    /// Tax Splitting Helper
    /// </summary>
    public static class TaxSplittingHelper
    {
        public static bool IsProductTaxSplittingOverridenByAttributeValue(ProductAttributeValue value, Product product)
        {
            return product != null && value != null && product.IsTaxSplitEnabled 
                   && (value.SplitAmount != default(decimal) || value.SplitAmount2 != default(decimal));
        }
    }
}