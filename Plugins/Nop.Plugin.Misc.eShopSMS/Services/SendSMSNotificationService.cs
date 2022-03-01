using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.eShopSMS.Data;
using Nop.Services.Localization;
using Nop.Services.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Misc.eShopSMS.Services
{
    public class SendSMSNotificationService : ISendSMSNotificationService
    {
        #region Field
        private readonly ISmsTemplateService _smsTemplateService;
        private readonly EShopSMSSettings _eShopSMSSettings;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor
        public SendSMSNotificationService(ISmsTemplateService smsTemplateService,
            EShopSMSSettings eShopSMSSettings,
            ILogger logger,
            ILocalizationService localizationService)
        {
            _smsTemplateService = smsTemplateService;
            _eShopSMSSettings = eShopSMSSettings;
            _logger = logger;
            _localizationService = localizationService;
        }
        #endregion

        #region Utilities
        private string PrepareMessage(string templateBody,Order order)
        {
            templateBody = templateBody.Replace("%customer%", order.BillingAddress.FirstName);
            templateBody = templateBody.Replace("%Order.OrderNumber%", order.Id.ToString());

            return templateBody;
        }
        #endregion

        #region Methods
        public void SendCustomerNotification(string sMSTemplateName,Order order)
        {
            var smsTemplates = _smsTemplateService.GetSmsTemplatesByName(sMSTemplateName);
            if (smsTemplates != null)
            {
                var smsTemplate = smsTemplates.Where(p => p.IsActive).OrderBy(p => p.CreatedOn).LastOrDefault();
                if (smsTemplate != null)
                {
                    var body = _localizationService.GetLocalized(smsTemplate, mt => mt.Body, order.CustomerLanguageId);
                    var templateBody = PrepareMessage(body, order);
                    //var client = new RestClient("http://10.90.100.38:8080/urlpserver/URLP_285?origin_addr=Orange&mobile_no=+962770386635&msg=Testmessage&username=SSO&password=SSO%212016&msgtype=UTF-8");
                    var client = new RestClient($"{_eShopSMSSettings.APIUrl}?origin_addr=Orange&mobile_no={order.BillingAddress.PhoneNumber}&msg={templateBody}&username={_eShopSMSSettings.UserName}&password={_eShopSMSSettings.Password}&msgtype=UTF-8");
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    IRestResponse response = client.Execute(request);
                    if (response.IsSuccessful)
                    {
                        _logger.Information($"SMS Sent - {sMSTemplateName} Order: #{order.Id} Mobile:{order.BillingAddress.PhoneNumber}");
                    }
                    else
                    {
                        _logger.InsertLog(Core.Domain.Logging.LogLevel.Information, $"SMS sending error - {sMSTemplateName} Order: #{order.Id} Mobile:{order.BillingAddress.PhoneNumber}", "Content:" + response.Content + " Error:" + response.ErrorMessage);
                    }
                }
            }
        }
        
        #endregion
    }
}
