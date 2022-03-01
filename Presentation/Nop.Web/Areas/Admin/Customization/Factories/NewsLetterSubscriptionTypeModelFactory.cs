using System;
using System.Collections.Generic;
using Nop.Core.Domain.Messages;
using Nop.Services.Messages;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Framework.Factories;

namespace Nop.Web.Areas.Admin.Factories
{
    public interface INewsLetterSubscriptionTypeModelFactory
    {
        NewsLetterSubscriptionTypeSearchModel PrepareNewsLetterSubscriptionTypeSearchModel(NewsLetterSubscriptionTypeSearchModel searchModel);

        NewsLetterSubscriptionTypeListModel PrepareNewsLetterSubscriptionTypeListModel(NewsLetterSubscriptionTypeSearchModel searchModel);

        NewsLetterSubscriptionTypeModel PrepareNewsLetterSubscriptionTypeModel(NewsLetterSubscriptionTypeModel model,
            NewsLetterSubscriptionType newsLetterSubscriptionType, bool excludeProperties = false);
    }

    public class NewsLetterSubscriptionTypeModelFactory : INewsLetterSubscriptionTypeModelFactory
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly INewsLetterSubscriptionTypeService _newsLetterSubscriptionTypeService;

        #endregion

        #region Ctor

        public NewsLetterSubscriptionTypeModelFactory(ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            INewsLetterSubscriptionTypeService newsLetterSubscriptionTypeService)
        {
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _newsLetterSubscriptionTypeService = newsLetterSubscriptionTypeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare NewsLetterSubscriptionType search model
        /// </summary>
        /// <param name="searchModel">NewsLetterSubscriptionType search model</param>
        /// <returns>NewsLetterSubscriptionType search model</returns>
        public virtual NewsLetterSubscriptionTypeSearchModel PrepareNewsLetterSubscriptionTypeSearchModel(NewsLetterSubscriptionTypeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged NewsLetterSubscriptionType list model
        /// </summary>
        /// <param name="searchModel">NewsLetterSubscriptionType search model</param>
        /// <returns>NewsLetterSubscriptionType list model</returns>
        public virtual NewsLetterSubscriptionTypeListModel PrepareNewsLetterSubscriptionTypeListModel(NewsLetterSubscriptionTypeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get newsLetterSubscriptionType
            var newsLetterSubscriptionType = _newsLetterSubscriptionTypeService
                .GetNewsLetterSubscriptionTypes(searchModel.Page - 1, searchModel.PageSize);

            var spcGroupList = new List<NewsLetterSubscriptionTypeModel>();
            foreach (var spcGroup in newsLetterSubscriptionType)
            {
                spcGroupList.Add(new NewsLetterSubscriptionTypeModel
                {
                    Id = spcGroup.Id,
                    Name = _localizationService.GetLocalized(spcGroup, x => x.Name),
                    DisplayOrder = spcGroup.DisplayOrder
                });
            }

            //prepare list model
            var model = new NewsLetterSubscriptionTypeListModel
            {
                //fill in model values from the entity
                Data = spcGroupList,
                Total = newsLetterSubscriptionType.TotalCount
            };

            return model;
        }

        /// <summary>
        /// Prepare NewsLetterSubscriptionType model
        /// </summary>
        /// <param name="model">NewsLetterSubscriptionType model</param>
        /// <param name="NewsLetterSubscriptionType">NewsLetterSubscriptionType</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>NewsLetterSubscriptionType model</returns>
        public virtual NewsLetterSubscriptionTypeModel PrepareNewsLetterSubscriptionTypeModel(NewsLetterSubscriptionTypeModel model,
            NewsLetterSubscriptionType newsLetterSubscriptionType, bool excludeProperties = false)
        {
            Action<NewsLetterSubscriptionTypeLocalizedModel, int> localizedModelConfiguration = null;

            if (newsLetterSubscriptionType != null)
            {
                //fill in model values from the entity
                if (model != null)
                {
                    model.Id = newsLetterSubscriptionType.Id;
                    model.Name = newsLetterSubscriptionType.Name;
                    model.DisplayOrder = newsLetterSubscriptionType.DisplayOrder;
                }
                else
                {
                    model = new NewsLetterSubscriptionTypeModel()
                    {
                        Id = newsLetterSubscriptionType.Id,
                        Name = newsLetterSubscriptionType.Name,
                        DisplayOrder = newsLetterSubscriptionType.DisplayOrder
                    };
                }

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(newsLetterSubscriptionType, entity => entity.Name, languageId, false, false);
                };
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }
        #endregion
    }
}
