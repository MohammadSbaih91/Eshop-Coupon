using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Widgets.CustomerOrderReview.Domain;

namespace Widgets.CustomerOrderReview.Models
{
    public partial class CustomerOrderReviewModel : BaseNopEntityModel
    {
        public int StoreId { get; set; }
        public int ReviewId { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public int Rate1 { get; set; }
        public int Rate2 { get; set; }
        public int Rate3 { get; set; }
        public int Rate4 { get; set; }
        public string Feedback { get; set; }
        public CustomerOrderReviewType CustomerOrderReviewType { get; set; }
        public string CustomerOrderReviewString => CustomerOrderReviewType.ToString();

        public string Rate1Label { get; set; }
        public string Rate2Label { get; set; }
        public string Rate3Label { get; set; }
        public string Rate4Label { get; set; }
        public string FeedbackLabel { get; set; }

        public bool HideRatingForm =>
            string.IsNullOrWhiteSpace(Rate1Label)
            && string.IsNullOrWhiteSpace(Rate2Label)
            && string.IsNullOrWhiteSpace(Rate3Label)
            && string.IsNullOrWhiteSpace(Rate4Label)
            && string.IsNullOrWhiteSpace(FeedbackLabel);
        
        [NopResourceDisplayName("Plugins.CustomerOrderReview.Store")]
        public string StoreName { get; set; }
        [NopResourceDisplayName("Plugins.CustomerOrderReview.Customer")]
        public string CustomerInfo { get; set; }

        [NopResourceDisplayName("Plugins.CustomerOrderReview.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}