namespace Nop.Core.Domain.Catalog
{
    public partial class Category
    {
        public int CategoryProductBoxTemplateId { get; set; }

        public bool ShowWithSubCategories { get; set; }
        public decimal MinimumPriceOfProduct { get; set; }
    }
}
