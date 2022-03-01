using Nop.Plugin.Misc.eShopSMS.Areas.Admin.Models;
using Nop.Plugin.Misc.eShopSMS.Domains;
using Nop.Services.Localization;
using Nop.Web.Framework.Factories;
using System;

namespace Nop.Plugin.Misc.eShopSMS.Areas.Admin.Factories
{
    public interface ISmsTemplateModelFactory
    {
        SmsTemplateModel PrepareSmsTemplateModel(SmsTemplateModel model,
            SMSTemplate smsTemplate);
    }

    public class SmsTemplateModelFactory : ISmsTemplateModelFactory
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        #endregion

        #region Ctor
        public SmsTemplateModelFactory(ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory)
        {
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prepare sms template model
        /// </summary>
        /// <param name="model">sms template model</param>
        /// <param name="smsTemplate">sms template</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>sms template model</returns>
        public virtual SmsTemplateModel PrepareSmsTemplateModel(SmsTemplateModel model,
            SMSTemplate smsTemplate)
        {
            Action<SmsTemplateLocalizedModel, int> localizedModelConfiguration = null;

            if (smsTemplate != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new SmsTemplateModel()
                    {
                        Id = smsTemplate.Id,
                        TemplateName = smsTemplate.TemplateName,
                        Body = smsTemplate.Body,
                        IsActive = smsTemplate.IsActive
                    };
                }
                
                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Body = _localizationService.GetLocalized(smsTemplate, entity => entity.Body, languageId, false, false);
                };
            }

            //prepare localized models
            model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);
            
            return model;
        }
        #endregion
    }
}
