
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Models
{
  public class AjaxCartButtonsModel
  {
    public AjaxCartButtonsModel() => AddProductToCartAjaxButtonModels = new List<AddProductToCartAjaxButtonModel>();

    public List<AddProductToCartAjaxButtonModel> AddProductToCartAjaxButtonModels { get; set; }
  }
}
