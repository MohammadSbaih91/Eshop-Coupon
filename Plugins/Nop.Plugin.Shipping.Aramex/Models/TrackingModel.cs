using FluentValidation.Attributes;
using Nop.Plugin.Shipping.Aramex.Validators;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Shipping.Aramex.Models
{
    [Validator(typeof(TrackingValidator))]
    public class TrackingModel : BaseNopModel
    {
        [NopResourceDisplayName("Shipping.Aramex.Tracking.OrderNumber")]
        public int? OrderNumber { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Shipping.Aramex.Tracking.Email")]
        public string Email { get; set; }
        
    }
}