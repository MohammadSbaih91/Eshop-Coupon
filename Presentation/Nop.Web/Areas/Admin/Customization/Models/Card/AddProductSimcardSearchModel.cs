using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Card
{
    public partial class AddProductSimcardSearchModel : BaseSearchModel
    {
        public string CardNumber { get; set; }
        public string Group { get; set; }
        public int ProductId { get; set; }
    }
}
