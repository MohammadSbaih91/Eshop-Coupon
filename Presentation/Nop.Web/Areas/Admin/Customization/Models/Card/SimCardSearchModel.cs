using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Card
{
    /// <summary>
    /// Represents a SimCard search model
    /// </summary>
    public partial class SimCardSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.SimCard.Fields.CardNumber")]
        public string CardNumber { get; set; }
        [NopResourceDisplayName("Admin.SimCard.Fields.Group")]
        public string Group { get; set; }
    }
}
