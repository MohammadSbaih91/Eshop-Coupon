using Nop.Core.Domain.Catalog;
using System.Collections.Generic;

namespace Nop.Web.Models.Catalog
{
    public partial class ProductOverviewModel
    {
        public bool OneMonthDiscount { get; set; }
        public string SpecialPromotionDesc { get; set; }

        public bool SpecialPromotion { get; set; }
        public string DiscountDesc { get; set; }

        public decimal TaxRate { get; set; }
        public string TaxDesc { get; set; }
        public int TaxCategoryId { get; set; }
        public string PriceWithoutDiscount { get; set; }

        public IList<ProductAttributeOverviewModel> ProductAttributeOverviewModels { get; set; } = new List<ProductAttributeOverviewModel>();

        public bool IsOutOfStock { get; set; }

        public string ButtonName { get; set; }

        public bool IsServiceProductAddedToCart { get; set; }
        public bool ManageInventoryMethod { get; set; }
    }

    public partial class ProductAttributeOverviewModel
    {
        public ProductAttributeOverviewModel()
        {
            Values = new List<ProductAttributeValueOverviewModel>();
        }
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ProductAttributeId { get; set; }
        public int AttributeControlTypeId { get; set; }
        public string Name { get; set; }
        public string TextPrompt { get; set; }
        public AttributeControlType AttributeControlType
        {
            get => (AttributeControlType)AttributeControlTypeId;
            set => AttributeControlTypeId = (int)value;
        }
        public int DisplayOrder { get; set; }

        public List<ProductAttributeValueOverviewModel> Values { get; set; }
    }

    public partial class ProductAttributeValueOverviewModel
    {
        public int Id { get; set; }

        public int ProductAttributeMappingId { get; set; }

        public int AttributeValueTypeId { get; set; }

        public string Name { get; set; }

        public string ColorSquaresRgb { get; set; }

        public bool IsPreSelected { get; set; }
        public string Price { get; set; }
        public decimal PriceValue { get; set; }
    }
}
