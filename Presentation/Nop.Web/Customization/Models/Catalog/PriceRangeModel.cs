namespace Nop.Web.Models.Catalog
{
    public class PriceRangeModel
    {
        public int CategoryId { get; set; }

        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        
        public decimal CurrentMinPrice { get; set; }

        public decimal CurrentMaxPrice { get; set; }
    }
}
