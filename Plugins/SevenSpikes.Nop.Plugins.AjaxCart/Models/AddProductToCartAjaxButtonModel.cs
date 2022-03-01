
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Models
{
  public class AddProductToCartAjaxButtonModel
  {
    public AddProductToCartAjaxButtonModel() => AllowedQuantities = new List<SelectListItem>();

    public int ProductId { get; set; }

    public bool IsProductPage { get; set; }

    public int DefaultProductMinimumQuantity { get; set; }

    public bool ShouldAddSettings { get; set; }

    public string ButtonValue { get; set; }

    public List<SelectListItem> AllowedQuantities { get; set; }
  }
}
