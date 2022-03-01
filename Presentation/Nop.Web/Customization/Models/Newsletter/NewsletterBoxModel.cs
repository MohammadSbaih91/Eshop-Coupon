using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Nop.Web.Models.Newsletter
{
    public partial class NewsletterBoxModel
    {
        public int NewsLetterSubscriptionTypeId { get; set; }
        public IList<SelectListItem> AvailableNewsLetterSubscriptionTypes { get; set; } = new List<SelectListItem>();
    }
}
