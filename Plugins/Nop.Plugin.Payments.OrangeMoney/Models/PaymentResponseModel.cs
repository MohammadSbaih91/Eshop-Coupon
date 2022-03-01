using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.OrangeMoney.Models
{
    public partial class PaymentResponseModel
    {
        public string merchantTrxNo { get; set; }
        public string payTrxStatus { get; set; }
        public string description { get; set; }
        public string payAmount { get; set; }
        public string payTrxDate { get; set; }
        public string payTrxNo { get; set; }
    }
}
