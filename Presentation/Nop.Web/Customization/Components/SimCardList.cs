using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Card;
using Nop.Web.Framework.Components;
using Nop.Web.Models.SimCard;
using System;

namespace Nop.Web.Components
{
    public class SimCardListViewComponent : NopViewComponent
    {
        private readonly ISimCardService _simCardService;

        public SimCardListViewComponent(ISimCardService simCardService)
        {
            this._simCardService = simCardService;
        }

        public IViewComponentResult Invoke(int productId)
        {
            var model = new SimCardListModel();
            var simcards = _simCardService.GetSimCardListByProductId(productId);

            foreach (var si in simcards)
            {
                model.SimCard.Add(new SelectListItem
                {
                    Text = si.CardNumber,
                    Value = Convert.ToString(si.Id)
                });
            }
            return View(model);
        }
    }
}