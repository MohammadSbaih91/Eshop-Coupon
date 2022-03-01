using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using System.Collections.Generic;

namespace Nop.Web.Models.Order
{
    public partial class OrderDetailsModel : BaseNopEntityModel
    {
        //public IList<OrderItemModel> orderItemModels { get; set; } = new List<OrderItemModel>();

        public partial class OrderItemModel
        {
            public PictureModel PictureModel { get; set; }
            public ProductReviewOverviewModel ReviewOverviewModel { get; set; }
        }
    }
}
