using Nop.Core.Domain.Payments;

namespace Nop.Web.Models.Checkout
{
    public partial class CheckoutCompletedModel
    {
        public PaymentStatus PaymentStatus { get; set; }
    }
}
