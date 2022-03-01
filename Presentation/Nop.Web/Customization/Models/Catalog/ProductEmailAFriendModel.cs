using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Models.Catalog
{
    public partial class ProductEmailAFriendModel  
    {
        [NopResourceDisplayName("Products.EmailAFriend.FullName")]
        public string FullName { get; set; }
    }
}