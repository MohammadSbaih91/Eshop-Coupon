using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial class ManufacturerNavigationModel
    {
        public int Categoryid { get; set; }
    }

    public partial class ManufacturerBriefInfoModel
    {
        public PictureModel PictureModel { get; set; } = new PictureModel();

    }
}
