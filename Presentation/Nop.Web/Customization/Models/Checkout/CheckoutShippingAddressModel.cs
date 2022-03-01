using Nop.Web.Models.Common;

namespace Nop.Web.Models.Checkout
{
    public partial class CheckoutShippingAddressModel
    {
        public AddressModel BillingNewAddress { get; set; } = new AddressModel();
        public bool ShipToSameAddress { get; set; } = true;
    }
}
