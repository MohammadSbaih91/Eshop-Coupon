using Nop.Core.Configuration;

namespace Widgets.CustomerOrderReview
{
    public class CustomerOrderReviewSettings : ISettings
    {
        public string Rate1Label { get; set; }
        public string Rate2Label { get; set; }
        public string Rate3Label { get; set; }
        public string Rate4Label { get; set; }
        public string FeedbackLabel { get; set; }
    }
}