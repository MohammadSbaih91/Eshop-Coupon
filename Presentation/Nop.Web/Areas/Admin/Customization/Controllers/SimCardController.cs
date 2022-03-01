using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Localization;
using Nop.Services.Card;
using Nop.Web.Areas.Admin.Models.Card;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using Nop.Core;
using System.Collections.Generic;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Services.Orders;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class SimCardController : BaseAdminController
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly ISimCardService _simCardService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        #endregion Fields

        #region Ctor

        public SimCardController(ILocalizationService localizationService,
            ISimCardService simCardService,
            IPermissionService permissionService,
            IOrderService orderService)
        {
            _localizationService = localizationService;
            _simCardService = simCardService;
            _permissionService = permissionService;
            _orderService = orderService;
        }

        #endregion

        #region Methods
        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            var model = new SimCardSearchModel();
            //prepare page parameters
            model.SetGridPageSize();
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(SimCardSearchModel searchModel)
        {
            //prepare model
            var simcards = _simCardService.GetSimCards(searchModel.CardNumber, searchModel.Group,0, searchModel.Page - 1, searchModel.PageSize);
            var model = new SimCardListModel
            {
                //fill in model values from the entity
                Data = simcards.Select(simcard => new SimCardModel
                {
                    Id = simcard.Id,
                    CardNumber = simcard.CardNumber,
                    DisplayOrder = simcard.DisplayOrder,
                    Group = simcard.Group,
                    IsSale = simcard.IsSale
                }),
                Total = simcards.Count()
            };

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult SimCardUpdate(SimCardModel model)
        {
            if (model.CardNumber != null)
                model.CardNumber = model.CardNumber.Trim();

            if (model.Group != null)
                model.Group = model.Group.Trim();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var simcard = _simCardService.GetSimCardById(model.Id);

            // if the card number changed, ensure it isn't being used by another card number
            if (!simcard.CardNumber.Equals(model.CardNumber, StringComparison.InvariantCultureIgnoreCase))
            {
                var card = _simCardService.GetSimCardByNumber(model.CardNumber);
                if (card != null && card.Id != simcard.Id)
                {
                    return Json(new DataSourceResult { Errors = string.Format(_localizationService.GetResource("Admin.SimCard.Fields.CardNumber.NumberAlreadyExists"), card.CardNumber) });
                }
            }

            simcard.CardNumber = model.CardNumber;
            simcard.DisplayOrder = model.DisplayOrder;
            simcard.Group = model.Group;
            simcard.UpdatedOnUtc = DateTime.UtcNow;
            simcard.IsSale = false;
            _simCardService.UpdateSimCard(simcard);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult SimCardAdd(SimCardModel model)
        {
            if (model.CardNumber != null)
                model.CardNumber = model.CardNumber.Trim();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var card = _simCardService.GetSimCardByNumber(model.CardNumber);
            if (card == null)
            {
                var simCard = new SimCard()
                {
                    CardNumber = model.CardNumber,
                    DisplayOrder = model.DisplayOrder,
                    Group = model.Group,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    IsSale = false
                };

                _simCardService.InsertSimCard(simCard);
            }
            else
            {
                return Json(new DataSourceResult { Errors = string.Format(_localizationService.GetResource("Admin.SimCard.Fields.CardNumber.NumberAlreadyExists"), card.CardNumber) });
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult SimCardDelete(int id)
        {
            //try to get a Sim card with the specified id
            var card = _simCardService.GetSimCardById(id)
                ?? throw new ArgumentException("No sim card with the specified id", nameof(id));

            _simCardService.DeleteSimCard(card);

            return new NullJsonResult();
        }
        #endregion

        #region Export / Import
        [HttpPost]
        public virtual IActionResult ImportFromXlsx(IFormFile importexcelfile)
        {
            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    using (var xlPackage = new ExcelPackage(importexcelfile.OpenReadStream()))
                    {
                        // get the first worksheet in the workbook
                        var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                            throw new NopException("No worksheet found");

                        int totalRows = worksheet.Dimension.Rows;

                        var simCards = new List<SimCard>();

                        for (int i = 2; i <= totalRows; i++)
                        {
                            var cardNumber = worksheet.Cells[i, 1].Value != null ? worksheet.Cells[i, 1].Value.ToString() : null;

                            var card = simCards.Where(x => x.CardNumber == cardNumber).FirstOrDefault();
                            if (card != null)
                                continue;

                            var simCardNumber = _simCardService.GetSimCardByNumber(cardNumber);
                            if (simCardNumber != null)
                                continue;

                            simCards.Add(new SimCard()
                            {
                                CardNumber = cardNumber,
                                Group = worksheet.Cells[i, 2].Value != null ? worksheet.Cells[i, 2].Value.ToString() : null,
                                DisplayOrder = worksheet.Cells[i, 3].Value != null ? Convert.ToInt32(worksheet.Cells[i, 3].Value.ToString()) : 0,
                                IsSale = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                        }
                        _simCardService.InsertSimCards(simCards);
                    }
                }
                else
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }
                SuccessNotification(_localizationService.GetResource("Admin.SimCard.Imported"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }
        #endregion

        #region Product Simcard Mapping
        [HttpPost]
        public IActionResult ProductSimcardList(int productId)
        {
            var productSimcards = _simCardService.GetProductSimcardByProductId(productId);
            var productSimcardList = new List<ProductSimcardModel>();
            if(productSimcards != null && productSimcards.Count > 0)
            {
                foreach (var productSimcard in productSimcards)
                {                    
                    var simcard = _simCardService.GetSimCardById(productSimcard.SimCardId);
                    productSimcardList.Add(new ProductSimcardModel()
                    {
                        Id = productSimcard.Id,
                        ProductId = productSimcard.ProductId,
                        SimcardId = productSimcard.SimCardId,
                        CardNumber = simcard.CardNumber,
                        Group = simcard.Group,
                        IsSale = simcard.IsSale
                    });
                }
            }
            var model = new ProductSimcardListModel()
            {
                Data = productSimcardList,
                Total = productSimcards.Count
            };
            return Json(model);
        }

        public IActionResult ProductSimcardAddPopup(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            var searchModel = new AddProductSimcardSearchModel();
            searchModel.ProductId = productId;
            searchModel.SetPopupGridPageSize();
            return View(searchModel);
        }

        [HttpPost]
        public IActionResult ProductSimcardAddPopupList(AddProductSimcardSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            var simcards = _simCardService.GetSimCardsList(searchModel.CardNumber, searchModel.Group,searchModel.ProductId, searchModel.Page - 1, searchModel.PageSize);
            var model = new SimCardListModel
            {
                //fill in model values from the entity
                Data = simcards.Select(simcard => new SimCardModel
                {
                    Id = simcard.Id,
                    CardNumber = simcard.CardNumber,
                    DisplayOrder = simcard.DisplayOrder,
                    Group = simcard.Group
                }),
                Total = simcards.Count()
            };

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual IActionResult ProductSimcardAddPopup(AddProductSimcardModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if(model.SelectedSimcardIds != null && model.SelectedSimcardIds.Count > 0 && model.ProductId > 0)
            {
                foreach (var item in model.SelectedSimcardIds)
                {
                        var productSimcardMapping = new ProductSimcardMapping();
                        productSimcardMapping.ProductId = model.ProductId;
                        productSimcardMapping.SimCardId = item;
                        _simCardService.InsertProductsimcard(productSimcardMapping);                    
                }
            }
            ViewBag.RefreshPage = true;
            return View(new AddProductSimcardSearchModel());
        }

        [HttpPost]
        public virtual IActionResult ProductSimCardDelete(int id)
        {
            //try to get a Sim card with the specified id
            var productSimcard = _simCardService.GetProductSimCardById(id)
                ?? throw new ArgumentException("No sim card with the specified id", nameof(id));

            _simCardService.DeleteProductsimcard(productSimcard);

            return new NullJsonResult();
        }
        #endregion
    }
}
