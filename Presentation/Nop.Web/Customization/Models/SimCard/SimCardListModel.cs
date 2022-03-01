using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Nop.Web.Models.SimCard
{
    public class SimCardListModel
    {
        public SimCardListModel()
        {
            SimCard = new List<SelectListItem>();
        }

        public IList<SelectListItem> SimCard { get; set; }
    }
}
