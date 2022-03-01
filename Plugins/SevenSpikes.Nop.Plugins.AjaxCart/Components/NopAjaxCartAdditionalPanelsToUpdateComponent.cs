
using Microsoft.AspNetCore.Mvc;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Components
{
  [ViewComponent(Name = "NopAjaxCartAdditionalPanelsToUpdate")]
  public class NopAjaxCartAdditionalPanelsToUpdateComponent : ViewComponent
  {
    public IViewComponentResult Invoke() => View("AdditionalPanelsToUpdate");
  }
}
