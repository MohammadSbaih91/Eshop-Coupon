using Nop.Plugin.Misc.eShopSMS.Domains;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.eShopSMS.Services
{
    public interface ISmsTemplateService
    {
        /// <summary>
        /// Delete a SMS template
        /// </summary>
        /// <param name="SmsTemplate">SMS template</param>
        void DeleteSMSTemplate(SMSTemplate sMSTemplate);

        /// <summary>
        /// Inserts a SMS template
        /// </summary>
        /// <param name="smsTemplate">SMS template</param>
        void InsertSMSTemplate(SMSTemplate sMSTemplate);

        /// <summary>
        /// Updates a sms template
        /// </summary>
        /// <param name="smsTemplate">SMS template</param>
        void UpdateSMSTemplate(SMSTemplate sMSTemplate);

        /// <summary>
        /// Gets a sms template
        /// </summary>
        /// <param name="smsTemplateId">sms template identifier</param>
        /// <returns>sms template</returns>
        SMSTemplate GetSmsTemplateById(int smsTemplateId);

        /// <summary>
        /// Gets sms templates by the name
        /// </summary>
        /// <param name="smsTemplateName">Sms template name</param>
        IList<SMSTemplate> GetSmsTemplatesByName(string smsTemplateName);

        /// <summary>
        /// Gets all sms templates
        /// </summary>
        /// <returns>sms template list</returns>
        IList<SMSTemplate> GetAllSmsTemplates();
    }
}
