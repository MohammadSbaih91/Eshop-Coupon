using System;
using Nop.Core;

namespace Widgets.CustomerOrderReview.Domain
{
    /// <summary>
    /// CustomerOrderReview
    /// </summary>
    public partial class CustomerOrderReview : BaseEntity
    {
        public int StoreId { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public int Rate1 { get; set; }
        public int Rate2 { get; set; }
        public int Rate3 { get; set; }
        public int Rate4 { get; set; }
        public string Feedback { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public int CustomerOrderReviewTypeId { get; set; }

        public CustomerOrderReviewType CustomerOrderReviewType
        {
            get => (CustomerOrderReviewType) CustomerOrderReviewTypeId;
            set => CustomerOrderReviewTypeId = (int) value;
        }
    }
}