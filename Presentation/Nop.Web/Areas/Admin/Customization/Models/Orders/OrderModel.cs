using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Orders
{
    public partial class OrderModel
    {
        [NopResourceDisplayName("Admin.Orders.Fields.StudentID")]
        public string StudentID { get; set; }

        public bool IsStudentIdNeeded { get; set; }

        [NopResourceDisplayName("Admin.Orders.Fields.UploadStudentID")]
        public string UploadStudentID { get; set; }

        [NopResourceDisplayName("Admin.Orders.Fields.UploadID")]
        public string UploadID { get; set; }

        [NopResourceDisplayName("Admin.Orders.Fields.BuildingNo")]
        public string BuildingNo { get; set; }
    }
}
