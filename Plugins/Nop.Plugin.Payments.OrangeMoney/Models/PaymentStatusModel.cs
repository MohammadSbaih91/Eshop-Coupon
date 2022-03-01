using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.OrangeMoney.Models
{
    public class PaymentStatusModel : BaseNopModel
    {
        public int OrderId { get; set; }
        public bool CanRePostProcessPayment { get; set; }
        public bool IsPaid { get; set; }
        public string FailStatusMessage { get; set; }
    }
}