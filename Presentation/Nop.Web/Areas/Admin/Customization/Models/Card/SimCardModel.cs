using FluentValidation.Attributes;
using Nop.Web.Areas.Admin.Validators.Card;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Card
{
    /// <summary>
    /// Represents a SimCard model
    /// </summary>
    [Validator(typeof(SimCardValidator))]
    public partial class SimCardModel : BaseNopEntityModel
    {

        #region Properties

        [NopResourceDisplayName("Admin.SimCard.Fields.CardNumber")]
        public string CardNumber { get; set; }

        [NopResourceDisplayName("Admin.SimCard.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.SimCard.Field.Group")]
        public string Group { get; set; }
        public bool IsSale { get; set; }

        #endregion
    }

}
