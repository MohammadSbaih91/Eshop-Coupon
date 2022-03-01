using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Plugin.Misc.eShopSMS.Domains;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Misc.eShopSMS.Services
{
    public class SmsTemplateService : ISmsTemplateService
    {
        #region Fields
        private readonly IRepository<SMSTemplate> _repositorySMSTemplate;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        #endregion

        #region Ctor
        public SmsTemplateService(IRepository<SMSTemplate> repositorySMSTemplate,
            ICacheManager cacheManager,
            IEventPublisher eventPublisher
            )
        {
            _repositorySMSTemplate = repositorySMSTemplate;
            _cacheManager = cacheManager;
            _eventPublisher = eventPublisher;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Delete a SMS template
        /// </summary>
        /// <param name="SmsTemplate">SMS template</param>
        public virtual void DeleteSMSTemplate(SMSTemplate sMSTemplate)
        {
            if (sMSTemplate == null)
                throw new ArgumentNullException(nameof(sMSTemplate));

            _repositorySMSTemplate.Delete(sMSTemplate);

            _cacheManager.RemoveByPattern(SmsDefaults.SmsTemplatesPatternCacheKey);

            //event notification
            _eventPublisher.EntityDeleted(sMSTemplate);
        }

        /// <summary>
        /// Inserts a SMS template
        /// </summary>
        /// <param name="smsTemplate">SMS template</param>
        public virtual void InsertSMSTemplate(SMSTemplate sMSTemplate)
        {
            if (sMSTemplate == null)
                throw new ArgumentNullException(nameof(sMSTemplate));

            _repositorySMSTemplate.Insert(sMSTemplate);

            _cacheManager.RemoveByPattern(SmsDefaults.SmsTemplatesPatternCacheKey);

            //event notification
            _eventPublisher.EntityInserted(sMSTemplate);
        }

        /// <summary>
        /// Updates a sms template
        /// </summary>
        /// <param name="smsTemplate">SMS template</param>
        public virtual void UpdateSMSTemplate(SMSTemplate sMSTemplate)
        {
            if (sMSTemplate == null)
                throw new ArgumentNullException(nameof(sMSTemplate));

            _repositorySMSTemplate.Update(sMSTemplate);

            _cacheManager.RemoveByPattern(SmsDefaults.SmsTemplatesPatternCacheKey);

            //event notification
            _eventPublisher.EntityUpdated(sMSTemplate);
        }

        /// <summary>
        /// Gets a sms template
        /// </summary>
        /// <param name="smsTemplateId">sms template identifier</param>
        /// <returns>sms template</returns>
        public virtual SMSTemplate GetSmsTemplateById(int smsTemplateId)
        {
            if (smsTemplateId == 0)
                return null;

            return _repositorySMSTemplate.GetById(smsTemplateId);
        }

        /// <summary>
        /// Gets sms templates by the name
        /// </summary>
        /// <param name="smsTemplateName">Sms template name</param>
        public virtual IList<SMSTemplate> GetSmsTemplatesByName(string smsTemplateName)
        {
            if (string.IsNullOrWhiteSpace(smsTemplateName))
                throw new ArgumentException(nameof(smsTemplateName));

            var key = string.Format(SmsDefaults.SmsTemplatesByNameCacheKey, smsTemplateName);
            return _cacheManager.Get(key, () =>
            {
                //get sms templates with the passed name
                var templates = _repositorySMSTemplate.Table
                    .Where(smsTemplate => smsTemplate.TemplateName.Equals(smsTemplateName))
                    .OrderBy(smsTemplate => smsTemplate.Id).ToList();
                
                return templates;
            });
        }

        /// <summary>
        /// Gets all sms templates
        /// </summary>
        /// <returns>sms template list</returns>
        public virtual IList<SMSTemplate> GetAllSmsTemplates()
        {
            var key = string.Format(SmsDefaults.SmsTemplatesAllCacheKey);
            return _cacheManager.Get(key, () =>
            {
                var query = _repositorySMSTemplate.Table;
                query = query.OrderBy(t => t.TemplateName);
                
                query = query.Distinct().OrderBy(t => t.TemplateName);

                return query.ToList();
            });
        }
        #endregion
    }
}
