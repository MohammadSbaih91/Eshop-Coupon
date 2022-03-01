using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Common;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Common
{
    public partial class AddressModel  
    {
        [NopResourceDisplayName("Address.Fields.Nationality")]
        public Nationality Nationality { get; set; }  
        
        [NopResourceDisplayName("Address.Fields.NationalityType")]
        public NationalityType NationalityType { get; set; }
        
        [NopResourceDisplayName("Address.Fields.IdentityCardOrPassport")]
        public string IdentityCardOrPassport { get; set; }
             
        [NopResourceDisplayName("Address.Fields.Civility")]
        public Civility Civility{ get; set; }
        
        [NopResourceDisplayName("Address.Fields.EmailConfirm")]
        public string EmailConfirm { get; set; }
        
        [NopResourceDisplayName("Address.Fields.SubscribeNewsletter")]
        public bool SubscribeNewsletter { get; set; }
        
        public string StudentID { get; set; }
        public bool IsStudentIdNeeded { get; set; }
        public string UploadStudentID { get; set; }
        public string UploadID { get; set; }
        public string BuildingNo { get; set; }

        public List<SelectListItem> Nationalities{ get; set; }
        public List<SelectListItem> NationalityTypes{ get; set; }
        
    }
}