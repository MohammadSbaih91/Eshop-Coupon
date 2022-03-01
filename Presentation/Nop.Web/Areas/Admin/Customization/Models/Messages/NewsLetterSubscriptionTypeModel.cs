using System.Collections.Generic;
using FluentValidation.Attributes;
using Nop.Web.Areas.Admin.Validators.Messages;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Messages
{
    /// <summary>
    /// Represents a NewsLetterSubscriptionTypeModel
    /// </summary>
    [Validator(typeof(NewsLetterSubscriptionTypeValidator))]
    public partial class NewsLetterSubscriptionTypeModel : BaseNopEntityModel, ILocalizedModel<NewsLetterSubscriptionTypeLocalizedModel>
    {
        #region Ctor

        public NewsLetterSubscriptionTypeModel()
        {
            Locales = new List<NewsLetterSubscriptionTypeLocalizedModel>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Messages.NewsLetterSubscriptionType.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Messages.NewsLetterSubscriptionType.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<NewsLetterSubscriptionTypeLocalizedModel> Locales { get; set; }
        
        #endregion
    }

    public partial class NewsLetterSubscriptionTypeLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Messages.NewsLetterSubscriptionType.Fields.Name")]
        public string Name { get; set; }
    }
}
