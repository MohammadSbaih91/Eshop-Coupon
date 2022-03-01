
using SevenSpikes.Nop.Framework.AutoMapper;
using SevenSpikes.Nop.Plugins.AjaxCart.Areas.Admin.Models;
using SevenSpikes.Nop.Plugins.AjaxCart.Domain;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Areas.Admin.Extensions
{
  public static class MappingExtensions
  {
    public static NopAjaxCartSettingsModel ToModel(this NopAjaxCartSettings nopAjaxFiltersSettings)
    {
      return nopAjaxFiltersSettings.MapTo<NopAjaxCartSettings, NopAjaxCartSettingsModel>();
    }

    public static NopAjaxCartSettings ToEntity(this NopAjaxCartSettingsModel nopAjaxFiltersSettingsModel)
    {
      return nopAjaxFiltersSettingsModel.MapTo<NopAjaxCartSettingsModel, NopAjaxCartSettings>();
    }
  }
}
