using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.Rss;
using Nop.Web.Framework.Security;
using Nop.Web.Models.Catalog;
using RestSharp;

namespace Nop.Web.Controllers
{
    public partial class ProductController
    {
        #region Utilities
       

        private ProductCategory GetCategoryByProductId(int productId)
        {
            var categoryService = EngineContext.Current.Resolve<ICategoryService>();
            return categoryService.GetProductCategoriesByProductId(productId).FirstOrDefault();
        }
        #endregion

        public const string PRODUCTS_REQUIRED_IDS_KEY = "Nop.pres.related.required-{0}-{1}";

        #region Email a friend
        [HttpPost, PublicAntiForgery]
        public virtual IActionResult ProductEmailAFriendSendWithFullName(ProductEmailAFriendModel model)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && !product.Deleted && product.Published && _catalogSettings.EmailAFriendEnabled)
            {
                //check whether the current customer is guest and ia allowed to email a friend
                if (_workContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToEmailAFriend)
                {
                    ModelState.AddModelError("",
                        _localizationService.GetResource("Products.EmailAFriend.OnlyRegisteredUsers"));
                }

                if (ModelState.IsValid)
                {
                    //email
                    _workflowMessageService.SendProductEmailAFriendMessage(_workContext.CurrentCustomer,
                        _workContext.WorkingLanguage.Id, product,
                        model.YourEmailAddress, model.FriendEmail,
                        Core.Html.HtmlHelper.FormatText(model.PersonalMessage, false, true, false, false, false, false)
                        , model.FullName);

                    model = _productModelFactory.PrepareProductEmailAFriendModel(model, product, true);
                    model.SuccessfullySent = true;
                    model.Result = _localizationService.GetResource("Products.EmailAFriend.SuccessfullySent");
                    return Json(model);
                }
                model = _productModelFactory.PrepareProductEmailAFriendModel(model, product, true);
            }

            model.SuccessfullySent = false;
            model.Result = _localizationService.GetResource("Products.EmailAFriend.FailedToSend");
            return Json(model);
        }
        #endregion


        public IActionResult RequiredAnyOneProductList(int productId, int? productThumbPictureSize)
        {
            var productIds = EngineContext.Current.Resolve<IStaticCacheManager>().Get(
                string.Format(PRODUCTS_REQUIRED_IDS_KEY, productId, _storeContext.CurrentStore.Id),
                () => _productService.ParseRequiredAnyOneFromOtherProductIds(_productService.GetProductById(productId)));

            //load products
            var products = _productService.GetProductsByIds(productIds);
            //load products
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();
            //visible individually
            products = products.Where(p => p.VisibleIndividually).ToList();

            if (!products.Any())
                return Content("");

            var model = _productModelFactory.PrepareProductOverviewModels(products, true, true, productThumbPictureSize)
                .ToList();
            return View(model);
        }

        public virtual IActionResult NewProductsRss()
        {
            var feed = new RssFeed(
                $"{_localizationService.GetLocalized(_storeContext.CurrentStore, x => x.Name)}: New products",
                "Information about products",
                new Uri(_webHelper.GetStoreLocation()),
                DateTime.UtcNow);

            if (!_catalogSettings.NewProductsEnabled)
                return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));

            var items = new List<RssItem>();

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                markedAsNewOnly: true,
                orderBy: ProductSortingEnum.CreatedOn,
                pageSize: _catalogSettings.NewProductsNumber);
            foreach (var product in products)
            {
                var productUrl = UrlStrucutre.UrlDecode(Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) }, _webHelper.CurrentRequestProtocol));
                var productName = _localizationService.GetLocalized(product, x => x.Name);
                var productDescription = _localizationService.GetLocalized(product, x => x.ShortDescription);
                var item = new RssItem(productName, productDescription, new Uri(productUrl), $"urn:store:{_storeContext.CurrentStore.Id}:newProducts:product:{product.Id}", product.CreatedOnUtc);
                items.Add(item);
                //uncomment below if you want to add RSS enclosure for pictures
                //var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                //if (picture != null)
                //{
                //    var imageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductDetailsPictureSize);
                //    item.ElementExtensions.Add(new XElement("enclosure", new XAttribute("type", "image/jpeg"), new XAttribute("url", imageUrl), new XAttribute("length", picture.PictureBinary.Length)));
                //}

            }
            feed.Items = items;
            return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));
        }

        //public virtual IActionResult AddProductToCompare(int categoryid)
        //{
        //    var model = new List<ProductOverviewModel>();
        //    if (categoryid > 0)
        //    {
        //        var categoryIds = new List<int>();
        //        categoryIds.Add(categoryid);
        //        var products = _productService.SearchProducts(categoryIds: categoryIds);

        //        model = _productModelFactory.PrepareProductOverviewModels(products).ToList();
        //    }

        //    return Json(new { html = RenderPartialViewToString("_CompareProducts_AddPopup", model) });
        //}

        public virtual IActionResult ClearCompareListAjax()
        {
            _compareProductsService.ClearCompareProducts();

            return Json(true);
        }

        public virtual IActionResult CheckProductWhileAddToCompare(int productId)
        {
            var IsShowMessage = false;

            var compareProducts = _compareProductsService.GetComparedProducts().FirstOrDefault();
            if (compareProducts == null)
                return Json(new
                {
                    result = IsShowMessage
                });

            var category = GetCategoryByProductId(productId);
            var compareedProductCategory = GetCategoryByProductId(compareProducts.Id);
            if (compareedProductCategory != null)
            {
                if (category.Category.Id == compareedProductCategory.Category.Id)
                {
                    IsShowMessage = false;
                }
                else
                {
                    IsShowMessage = true;
                }
            }
            return Json(new
            {
                result = IsShowMessage
            });
        }

        [HttpsRequirement(SslRequirement.No)]
        public virtual IActionResult CompareProducts()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("HomePage");

            var model = new CompareProductsModel
            {
                IncludeShortDescriptionInCompareProducts = _catalogSettings.IncludeShortDescriptionInCompareProducts,
                IncludeFullDescriptionInCompareProducts = _catalogSettings.IncludeFullDescriptionInCompareProducts,
            };

            var products = _compareProductsService.GetComparedProducts();

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();
            if (products != null && products.Count() > 0)
            {
                var product = products.FirstOrDefault();
                var category = GetCategoryByProductId(product.Id);
                if (category != null)
                {
                    var categoryModel = new CategoryModel()
                    {
                        Id = category.CategoryId,
                        Name = category.Category.Name,
                        SeName = _urlRecordService.GetSeName(category.Category)
                    };
                    model.CategoryModel = categoryModel;
                }

            }
            //prepare model
            _productModelFactory.PrepareProductOverviewModels(products, prepareSpecificationAttributes: true)
                .ToList()
                .ForEach(model.Products.Add);
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AddProductToCompareList(int productId, bool isAddedFromPopup = false, bool forceredirection = false)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published)
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            if (!_catalogSettings.CompareProductsEnabled)
                return Json(new
                {
                    success = false,
                    message = "Product comparison is disabled"
                });

            _compareProductsService.AddProductToCompareList(productId);

            //activity log
            _customerActivityService.InsertActivity("PublicStore.AddToCompareList",
                string.Format(_localizationService.GetResource("ActivityLog.PublicStore.AddToCompareList"), product.Name), product);

            if (isAddedFromPopup)
            {
                return Json(new
                {
                    success = true,
                    //use the code below (commented) if you want a customer to be automatically redirected to the compare products page
                    redirect = Url.RouteUrl("CompareProducts"),
                    isFromCompare = true
                });
            }
            else
            {
                if (forceredirection)
                {
                    return Json(new
                    {
                        success = true,
                        redirect = Url.RouteUrl("CompareProducts"),
                        isFromCompare = true
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = true,
                        message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToCompareList.Link"), Url.RouteUrl("CompareProducts")),
                        isFromCompare = true
                    });
                }
            }
        }

        [HttpPost]
        public virtual IActionResult AddProductReview(int productId, int ratingValue)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return Json(new
                {
                    result = false,
                    isApproved = false,
                    message = "",
                    redirectUrl = Url.RouteUrl("HomePage")
                });

            if (ratingValue < 1 || ratingValue > 5)
                ratingValue = _catalogSettings.DefaultProductRatingValue;

            if (!_productService.IsReviewAddeByCustomerToProduct(_workContext.CurrentCustomer.Id, productId))
            {
                var productReview = new ProductReview
                {
                    ProductId = product.Id,
                    CustomerId = _workContext.CurrentCustomer.Id,
                    //Title = model.AddProductReview.Title,
                    //ReviewText = model.AddProductReview.ReviewText,
                    Rating = ratingValue,
                    HelpfulYesTotal = 0,
                    HelpfulNoTotal = 0,
                    IsApproved = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    StoreId = _storeContext.CurrentStore.Id,
                };

                product.ProductReviews.Add(productReview);

                //update product totals
                _productService.UpdateProductReviewTotals(product);

                //notify store owner
                if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                    _workflowMessageService.SendProductReviewNotificationMessage(productReview, _localizationSettings.DefaultAdminLanguageId);

                //activity log
                _customerActivityService.InsertActivity("PublicStore.AddProductReview",
                    string.Format(_localizationService.GetResource("ActivityLog.PublicStore.AddProductReview"), product.Name), product);

                //raise event
                if (productReview.IsApproved)
                    _eventPublisher.Publish(new ProductReviewApprovedEvent(productReview));

                return Json(new
                {
                    result = true,
                    isApproved = true,
                    message = _localizationService.GetResource("Reviews.SuccessfullyAdded"),
                    redirectUrl = ""
                });
            }
            else
            {
                return Json(new
                {
                    result = true,
                    isApproved = false,
                    message = _localizationService.GetResource("Reviews.AlreadyAdded"),
                    redirectUrl = ""
                });
            }
        }

        #region sb

        // TODO: call api from client side when cors issue from gis is resolved.
        public virtual IActionResult ValidateLocationBasedService(string lat, string lng, string productType = "")
        {
            
            try
            {
                //if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lng))
                //    return Json("{}");

                var client = new RestClient($"http://10.1.166.134:8003/api/OrangeGISAvailability?lon={lng}&lat={lat}");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Content-Type", "application/json");
                var response = client.Execute(request);
                _logger.Warning(response.Content);
                return Json(response.Content);

            }
            catch (Exception e)
            {
                _logger.Warning(e.Message);
                return Json("{"+e.Message+"}");
            }
        }

        public virtual IActionResult GetLocationBasedService(string lat, string lng)
        {
            if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lng))
                return Json(new
                {
                    Success = false
                });
            var lang = EngineContext.Current.Resolve<IWorkContext>().WorkingLanguage.Id;
            string lannguage = "en";
            if (lang == 2)
            {
                lannguage = "ar";
            }
            var client = new RestClient($"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lng}&sensor=true&region=JOR&language={lannguage}&key=AIzaSyDJ6S-KUq5dBToqRnpOfZ88TnbPgW_M7ZE");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            IRestResponse response = client.Execute(request);
            if (request != null && !string.IsNullOrEmpty(response.Content))
            {
                //var googleMapResponse = JsonConvert.DeserializeObject<GoogleMapResponse>(response.Content);

                return Json(new
                {
                    Success = response.Content
                });
            }
            return Json(new
            {
                Success = false
            });
        }


        #endregion

    }
}