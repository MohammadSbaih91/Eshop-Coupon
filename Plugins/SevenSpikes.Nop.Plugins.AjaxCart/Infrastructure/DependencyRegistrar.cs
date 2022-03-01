
using SevenSpikes.Nop.Framework.DependancyRegistrar;
using SevenSpikes.Nop.Plugins.AjaxCart.Areas.Admin.Models;
using SevenSpikes.Nop.Plugins.AjaxCart.Domain;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Infrastructure
{
  public class DependencyRegistrar : BaseDependancyRegistrar7Spikes
  {
    protected override void CreateModelMappings() => CreateMvcModelMap<NopAjaxCartSettingsModel, NopAjaxCartSettings>();

    public DependencyRegistrar()
      : base()
    {
    }
  }
}
