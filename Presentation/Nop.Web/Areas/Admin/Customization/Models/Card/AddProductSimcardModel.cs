using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Card
{
    public class AddProductSimcardModel : BaseNopModel
    {
        public AddProductSimcardModel()
        {
            SelectedSimcardIds = new List<int>();
        }
        public int ProductId { get; set; }
        public IList<int> SelectedSimcardIds { get; set; }
    }
}
