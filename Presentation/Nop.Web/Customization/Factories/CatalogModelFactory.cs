using Nop.Core.Infrastructure;
using Nop.Services.Customization.Catalog;
using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using System.Collections.Generic;

namespace Nop.Web.Factories
{
    public partial interface ICatalogModelFactory
    {
        string PrepareCategoryProductBoxTemplateViewPath(int templateId);

        ManufacturerNavigationModel PrepareAllManufacturerNavigationModel(int categoryid = 0);

        PriceRangeModel PreparePriceRangeModel(int categoryId = 0, int manufacturerId = 0);


        CategoryNavigationModel PrepareCustomCategoryNavigationModel(int currentCategoryId);

        CatalogPagingFilteringModel PrepareCustomSpecificationAttributeFilter(int[] allfilterableSpecificationAttributeOptionIds,IList<int> alreadyFilteredSpecOptionIds, int[] filterableSpecificationAttributeOptionIds);

        List<CategoryModel> PrepareShowWithSubCategoryModels();

        List<CategoryModel> PrepareShowInHomePageCategoryModels();
    }

    public partial class CatalogModelFactory
    {

        /// <summary>
        /// Prepare category template view path
        /// </summary>
        /// <param name="templateId">Template identifier</param>
        /// <returns>Category template view path</returns>
        public virtual string PrepareCategoryProductBoxTemplateViewPath(int templateId)
        {
            var _categoryProductBoxTemplateService = EngineContext.Current.Resolve<ICategoryProductBoxTemplateService>();

            var template = _categoryProductBoxTemplateService.GetCategoryTemplateById(templateId);
            if (template == null)
                template = _categoryProductBoxTemplateService.GetAllCategoryTemplates().FirstOrDefault();
            if (template == null)
                throw new Exception("No default template could be loaded");
            
            return template.ViewPath;
        }


        /// <summary>
        /// Prepare manufacturer navigation model
        /// </summary>
        /// <param name="currentManufacturerId">Current manufacturer identifier</param>
        /// <returns>Manufacturer navigation model</returns>
        public virtual ManufacturerNavigationModel PrepareAllManufacturerNavigationModel(int categoryid = 0)
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_NAVIGATION_MODEL_KEY,
                categoryid,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
                var manufacturers = _manufacturerService.GetAllManufacturersByCategoryId(storeId: _storeContext.CurrentStore.Id,
                    pageSize: _catalogSettings.ManufacturersBlockItemsToDisplay,categoryId:categoryid);
                var model = new ManufacturerNavigationModel
                {
                    TotalManufacturers = manufacturers.TotalCount,
                    Categoryid = categoryid
                };

                foreach (var manufacturer in manufacturers)
                {
                    var modelMan = new ManufacturerBriefInfoModel
                    {
                        Id = manufacturer.Id,
                        Name = _localizationService.GetLocalized(manufacturer, x => x.Name),
                        SeName = _urlRecordService.GetSeName(manufacturer)
                    };

                    var pictureSize = _mediaSettings.ManufacturerThumbPictureSize;
                    var manufacturerPictureCacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_PICTURE_MODEL_KEY, manufacturer.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    modelMan.PictureModel = _cacheManager.Get(manufacturerPictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(manufacturer.PictureId);
                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageLinkTitleFormat"), modelMan.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageAlternateTextFormat"), modelMan.Name)
                        };
                        return pictureModel;
                    });
                    model.Manufacturers.Add(modelMan);
                }
                return model;
            });

            return cachedModel;
        }

        public virtual PriceRangeModel PreparePriceRangeModel(int categoryId = 0, int manufacturerId = 0)
        {
            var maxPrice = _productService.GetMaxPriceByCategoryId(categoryId, manufacturerId);
            var priceRange = _webHelper.QueryString<string>("prange");

            decimal currentMinPrice = 0, currentMaxPrice = maxPrice;
            if (!string.IsNullOrEmpty(priceRange))
            {
                var prices = priceRange.Split(",");
                if (prices.Length > 0)
                {
                    if (decimal.TryParse(prices[0], out decimal tmppriceMin))
                        currentMinPrice = tmppriceMin;
                }

                if (prices.Length > 1)
                {
                    if (decimal.TryParse(prices[1], out decimal tmppriceMax))
                        currentMaxPrice = tmppriceMax;
                }
            }

            return new PriceRangeModel()
            {
                CategoryId = categoryId,
                MinPrice = 0,
                MaxPrice = maxPrice,
                CurrentMinPrice = currentMinPrice,
                CurrentMaxPrice = currentMaxPrice
            };
        }

        public virtual CatalogPagingFilteringModel PrepareSpecificationAttributeFilter(IList<int> alreadyFilteredSpecOptionIds,int[] filterableSpecificationAttributeOptionIds)
        {
            var model = new CatalogPagingFilteringModel();
            var optionIds = filterableSpecificationAttributeOptionIds != null
                    ? string.Join(",", filterableSpecificationAttributeOptionIds) : string.Empty;
            var cacheKey = string.Format(ModelCacheEventConsumer.SPECS_FILTER_MODEL_KEY, optionIds, _workContext.WorkingLanguage.Id);

            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionsByIds(filterableSpecificationAttributeOptionIds);
            var allFilters = _cacheManager.Get(cacheKey, () => allOptions.Select(sao =>
                new SpecificationAttributeOptionFilter
                {
                    SpecificationAttributeId = sao.SpecificationAttribute.Id,
                    SpecificationAttributeName = _localizationService.GetLocalized(sao.SpecificationAttribute, x => x.Name, _workContext.WorkingLanguage.Id),
                    SpecificationAttributeDisplayOrder = sao.SpecificationAttribute.DisplayOrder,
                    SpecificationAttributeOptionId = sao.Id,
                    SpecificationAttributeOptionName = _localizationService.GetLocalized(sao, x => x.Name, _workContext.WorkingLanguage.Id),
                    SpecificationAttributeOptionColorRgb = sao.ColorSquaresRgb,
                    SpecificationAttributeOptionDisplayOrder = sao.DisplayOrder
                }).ToList());

            if (!allFilters.Any())
                return model;

            //sort loaded options
            allFilters = allFilters.OrderBy(saof => saof.SpecificationAttributeDisplayOrder)
                .ThenBy(saof => saof.SpecificationAttributeName)
                .ThenBy(saof => saof.SpecificationAttributeOptionDisplayOrder)
                .ThenBy(saof => saof.SpecificationAttributeOptionName).ToList();

            //prepare the model properties
            var removeFilterUrl = _webHelper.RemoveQueryString(_webHelper.GetThisPageUrl(true), "specs");
            model.SpecificationFilter.RemoveFilterUrl = ExcludeQueryStringParams(removeFilterUrl, _webHelper);

            //get already filtered specification options
            var alreadyFilteredOptions = allFilters.Where(x => alreadyFilteredSpecOptionIds.Contains(x.SpecificationAttributeOptionId));
            model.SpecificationFilter.AlreadyFilteredItems = alreadyFilteredOptions.Select(x =>
                new CatalogPagingFilteringModel.SpecificationFilterItem
                {
                    SpecificationAttributeName = x.SpecificationAttributeName,
                    SpecificationAttributeOptionName = x.SpecificationAttributeOptionName,
                    SpecificationAttributeOptionColorRgb = x.SpecificationAttributeOptionColorRgb
                }).ToList();

            //get not filtered specification options
            model.SpecificationFilter.NotFilteredItems = allFilters.Except(alreadyFilteredOptions).Select(x =>
            {
                //filter URL
                var alreadyFiltered = alreadyFilteredSpecOptionIds.Concat(new List<int> { x.SpecificationAttributeOptionId });
                var filterUrl = _webHelper.ModifyQueryString(_webHelper.GetThisPageUrl(true), "specs",
                    alreadyFiltered.OrderBy(id => id).Select(id => id.ToString()).ToArray());

                return new CatalogPagingFilteringModel.SpecificationFilterItem()
                {
                    SpecificationAttributeName = x.SpecificationAttributeName,
                    SpecificationAttributeOptionName = x.SpecificationAttributeOptionName,
                    SpecificationAttributeOptionColorRgb = x.SpecificationAttributeOptionColorRgb,
                    FilterUrl = ExcludeQueryStringParams(filterUrl, _webHelper)
                };
            }).ToList();

            return model;
        }

        /// <summary>
        /// Prepare category navigation model
        /// </summary>
        /// <param name="currentCategoryId">Current category identifier</param>
        /// <param name="currentProductId">Current product identifier</param>
        /// <returns>Category navigation model</returns>
        public virtual CategoryNavigationModel PrepareCustomCategoryNavigationModel(int currentCategoryId)
        {
            var currentCategory = _categoryService.GetCategoryById(currentCategoryId);
            var parentCategory = _categoryService.GetCategoryBreadCrumb(currentCategory).FirstOrDefault();

            var cId = currentCategoryId;
            var activeCategoryId = 0;
            if (parentCategory != null)
            {
                cId = parentCategory.Id;
                activeCategoryId = parentCategory.Id;
            }

            var childCategories = _categoryService.GetAllCategoriesByParentCategoryId(cId);
            //get active category
            
            if (currentCategoryId > 0)
            {
                //category details page
                activeCategoryId = currentCategoryId;
            }
            var cashedCategoriesModel = new List<CategorySimpleModel>();
            // Add parent Category for 'All'
            if (parentCategory != null)
            {
                var categoryModel = new CategorySimpleModel
                {
                    
                    Id = parentCategory.Id,
                    Name = _localizationService.GetResource("Common.All"),
                    SeName = _urlRecordService.GetSeName(parentCategory),
                    Desc = _localizationService.GetLocalized(parentCategory, x => x.Description)

                };
             
                cashedCategoriesModel.Add(categoryModel);
            }

            foreach (var childCategory in childCategories)
            {
                var categoryModel = new CategorySimpleModel
                {
                    Id = childCategory.Id,
                    Name = _localizationService.GetLocalized(childCategory, x => x.Name),
                    SeName = _urlRecordService.GetSeName(childCategory),
                    IncludeInTopMenu = childCategory.IncludeInTopMenu,
                    Desc = _localizationService.GetLocalized(childCategory, x => x.Description)
                };
                
                var subCategories = PrepareCategorySimpleModels(childCategory.Id, true);
                categoryModel.SubCategories.AddRange(subCategories);

                cashedCategoriesModel.Add(categoryModel);
            }

            //var cacheKey = string.Format("Nop.pres.category.all.custom-{0}-{1}-{2}",
            //    _workContext.WorkingLanguage.Id,
            //    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
            //    _storeContext.CurrentStore.Id);

            //var cashedCategoriesModel = _cacheManager.Get(cacheKey, () =>
            //{
            //    var categoriesModel = new List<CategorySimpleModel>();
            //    foreach (var childCategory in childCategories)
            //    {
            //        var categoryModel = new CategorySimpleModel
            //        {
            //            Id = childCategory.Id,
            //            Name = _localizationService.GetLocalized(childCategory, x => x.Name),
            //            SeName = _urlRecordService.GetSeName(childCategory),
            //            IncludeInTopMenu = childCategory.IncludeInTopMenu
            //        };

            //        var subCategories = PrepareCategorySimpleModels(childCategory.Id, true);
            //        categoryModel.SubCategories.AddRange(subCategories);

            //        categoriesModel.Add(categoryModel);
            //    }
            //    return categoriesModel;
            //});


            var model = new CategoryNavigationModel
            {
                CurrentCategoryId = activeCategoryId,
                Categories = cashedCategoriesModel
            };

            return model;
        }

        public virtual CatalogPagingFilteringModel PrepareCustomSpecificationAttributeFilter(int[] allfilterableSpecificationAttributeOptionIds, IList<int> alreadyFilteredSpecOptionIds, int[] filterableSpecificationAttributeOptionIds)
        {
            var model = new CatalogPagingFilteringModel();
            var optionIds = allfilterableSpecificationAttributeOptionIds != null
                    ? string.Join(",", allfilterableSpecificationAttributeOptionIds) : string.Empty;
            var cacheKey = string.Format(ModelCacheEventConsumer.SPECS_FILTER_MODEL_KEY, optionIds, _workContext.WorkingLanguage.Id);

            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionsByIds(allfilterableSpecificationAttributeOptionIds);
            var allFilters = _cacheManager.Get(cacheKey, () => allOptions.Select(sao =>
                new SpecificationAttributeOptionFilter
                {
                    SpecificationAttributeId = sao.SpecificationAttribute.Id,
                    SpecificationAttributeName = _localizationService.GetLocalized(sao.SpecificationAttribute, x => x.Name, _workContext.WorkingLanguage.Id),
                    SpecificationAttributeDisplayOrder = sao.SpecificationAttribute.DisplayOrder,
                    SpecificationAttributeOptionId = sao.Id,
                    SpecificationAttributeOptionName = _localizationService.GetLocalized(sao, x => x.Name, _workContext.WorkingLanguage.Id),
                    SpecificationAttributeOptionColorRgb = sao.ColorSquaresRgb,
                    SpecificationAttributeOptionDisplayOrder = sao.DisplayOrder
                }).ToList());

            if (!allFilters.Any())
                return model;

            //sort loaded options
            allFilters = allFilters.OrderBy(saof => saof.SpecificationAttributeDisplayOrder)
                .ThenBy(saof => saof.SpecificationAttributeName)
                .ThenBy(saof => saof.SpecificationAttributeOptionDisplayOrder)
                .ThenBy(saof => saof.SpecificationAttributeOptionName).ToList();

            //prepare the model properties
            var removeFilterUrl = _webHelper.RemoveQueryString(_webHelper.GetUrlReferrer(), "specs");
            model.SpecificationFilter.RemoveFilterUrl = ExcludeQueryStringParams(removeFilterUrl, _webHelper);

            //var alreadyFilteredOptions = allFilters.Where(x => alreadyFilteredSpecOptionIds.Contains(x.SpecificationAttributeOptionId));
            var filterList = new List<CatalogPagingFilteringModel.SpecificationFilterItem>();
            foreach (var item in allFilters)
            {
                //filter URL
                var filterUrl = "";
                if (alreadyFilteredSpecOptionIds.Contains(item.SpecificationAttributeOptionId))
                {
                    var ids = alreadyFilteredSpecOptionIds.Except(new List<int> { item.SpecificationAttributeOptionId }).ToList();

                    if (ids.Count > 0)
                    {
                        filterUrl = _webHelper.ModifyQueryString(_webHelper.GetUrlReferrer(), "specs",
                            ids.OrderBy(id => id).Select(id => id.ToString()).ToArray());
                    }
                    else
                        filterUrl = _webHelper.RemoveQueryString(_webHelper.GetUrlReferrer(), "specs");
                }
                else
                {
                    var alreadyFiltered = alreadyFilteredSpecOptionIds.Concat(new List<int> { item.SpecificationAttributeOptionId });
                    filterUrl = _webHelper.ModifyQueryString(_webHelper.GetUrlReferrer(), "specs",
                        alreadyFiltered.OrderBy(id => id).Select(id => id.ToString()).ToArray());
                }

                filterList.Add(new CatalogPagingFilteringModel.SpecificationFilterItem()
                {
                    SpecificationAttributeName = item.SpecificationAttributeName,
                    SpecificationAttributeOptionName = item.SpecificationAttributeOptionName,
                    SpecificationAttributeOptionColorRgb = item.SpecificationAttributeOptionColorRgb,
                    IsAlreadyFiltered = alreadyFilteredSpecOptionIds.Contains(item.SpecificationAttributeOptionId),
                    IsDisabled = !filterableSpecificationAttributeOptionIds.Contains(item.SpecificationAttributeOptionId),
                    FilterUrl = ExcludeQueryStringParams(filterUrl, _webHelper)
                });
            }
            model.SpecificationFilter.AllFilteredItems = filterList;
            return model;
        }

        public virtual List<CategoryModel> PrepareShowWithSubCategoryModels()
        {
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;

            var categoriesCacheKey = string.Format(EShopHelper.CacheKey_CategoryHomepageSubcategories,
                0,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                pictureSize,
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id,
                _webHelper.IsCurrentConnectionSecured());

            var model = _cacheManager.Get(categoriesCacheKey, () =>
                _categoryService.GetAllCategoriesShowWithSubCategories()
                .Select(category =>
                {
                    var catModel = new CategoryModel
                    {
                        Id = category.Id,
                        Name = _localizationService.GetLocalized(category, x => x.Name),
                        Description = _localizationService.GetLocalized(category, x => x.Description),
                        MetaKeywords = _localizationService.GetLocalized(category, x => x.MetaKeywords),
                        MetaDescription = _localizationService.GetLocalized(category, x => x.MetaDescription),
                        MetaTitle = _localizationService.GetLocalized(category, x => x.MetaTitle),
                        SeName = _urlRecordService.GetSeName(category),
                    };

                    //prepare picture model
                    var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, category.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(category.PictureId);
                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                        };
                        return pictureModel;
                    });

                    return catModel;
                })
                .ToList()
            );

            return model;
        }

        public virtual List<CategoryModel> PrepareShowInHomePageCategoryModels()
        {
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;

            var categoriesCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_HOMEPAGE_KEY,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                pictureSize,
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id,
                _webHelper.IsCurrentConnectionSecured());

            var model = _cacheManager.Get(categoriesCacheKey, () =>
                _categoryService.GetAllCategoriesShowWithSubCategories()
                .Select(category =>
                {
                    var catModel = new CategoryModel
                    {
                        Id = category.Id,
                        Name = _localizationService.GetLocalized(category, x => x.Name),
                        Description = _localizationService.GetLocalized(category, x => x.Description),
                        MetaKeywords = _localizationService.GetLocalized(category, x => x.MetaKeywords),
                        MetaDescription = _localizationService.GetLocalized(category, x => x.MetaDescription),
                        MetaTitle = _localizationService.GetLocalized(category, x => x.MetaTitle),
                        SeName = _urlRecordService.GetSeName(category),
                    };

                    //prepare picture model
                    var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, category.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(category.PictureId);
                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                        };
                        return pictureModel;
                    });

                    return catModel;
                })
                .ToList()
            );

            return model;
        }

        #region Utilities

        /// <summary>
        /// Exclude query string parameters
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="webHelper">Web helper</param>
        /// <returns>New URL</returns>
        protected virtual string ExcludeQueryStringParams(string url, IWebHelper webHelper)
        {
            //comma separated list of parameters to exclude
            const string excludedQueryStringParams = "pagenumber";
            var excludedQueryStringParamsSplitted = excludedQueryStringParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var exclude in excludedQueryStringParamsSplitted)
                url = webHelper.RemoveQueryString(url, exclude);
            return url;
        }

        #endregion
    }

}
