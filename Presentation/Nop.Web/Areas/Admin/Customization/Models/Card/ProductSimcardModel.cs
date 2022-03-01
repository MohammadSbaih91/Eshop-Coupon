using Nop.Web.Framework.Models;
using System.Collections;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Card
{
    public partial class ProductSimcardModel : BaseNopEntityModel
    {
        public int ProductId { get; set; }
        public int SimcardId { get; set; }
        public string CardNumber { get; set; }
        public string Group { get; set; }
        public bool IsSale { get; set; }
    }
}
