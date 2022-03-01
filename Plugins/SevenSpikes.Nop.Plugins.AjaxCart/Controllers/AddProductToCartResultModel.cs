
namespace SevenSpikes.Nop.Plugins.AjaxCart.Controllers
{
  public class AddProductToCartResultModel
  {
    public string PopupTitle { get; set; }

    public string Status { get; set; }

    public string AddToCartWarnings { get; set; }

    public string ErrorMessage { get; set; }

    public string RedirectUrl { get; set; }

    public string ProductAddedToCartWindow { get; set; }
  }
}
