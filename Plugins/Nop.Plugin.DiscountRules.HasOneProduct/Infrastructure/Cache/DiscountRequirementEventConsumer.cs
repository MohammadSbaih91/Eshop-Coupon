using Nop.Core.Domain.Discounts;
using Nop.Core.Events;
using Nop.Services.Configuration;
using Nop.Services.Events;

namespace Nop.Plugin.DiscountRules.HasOneProduct.Infrastructure.Cache
{
    /// <summary>
    /// Discount requirement rule event consumer (used for removing unused settings)
    /// </summary>
    public partial class DiscountRequirementEventConsumer : IConsumer<EntityDeletedEvent<DiscountRequirement>>
    {
        #region Fields
        
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public DiscountRequirementEventConsumer(ISettingService settingService)
        {
            this._settingService = settingService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle discount requirement deleted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        public void HandleEvent(EntityDeletedEvent<DiscountRequirement> eventMessage)
        {
            var discountRequirement = eventMessage?.Entity;
            if (discountRequirement == null)
                return;

            //delete saved restricted product identifiers if exist
            var setting = _settingService.GetSetting(string.Format(DiscountRequirementDefaults.SettingsKey, discountRequirement.Id));
            if (setting != null)
                _settingService.DeleteSetting(setting);
        }

        #endregion
    }
}