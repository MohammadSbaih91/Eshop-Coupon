using FluentValidation.Attributes;
using Nop.Web.Customization.Validators.EShopCommon;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Order;

namespace Nop.Web.Customization.Models
{
    [Validator(typeof(OrderTrackValidator))]
    public partial class OrderTrackModel
    {
        [NopResourceDisplayName("AnonymousOrderTrack.OrderId")]
        public int? OrderId{ get; set; }
        [NopResourceDisplayName("AnonymousOrderTrack.Email")]
        public string Email { get; set; }

        public OrderDetailsModel OrderDetailsModel { get; set; }=new OrderDetailsModel();
        
        public bool Success { get; set; }
    }
}