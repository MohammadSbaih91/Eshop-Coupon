using Nop.Plugin.Misc.AppointmentBooking.Areas.Admin.Models;
using Nop.Plugin.Misc.AppointmentBooking.Domains;
using Nop.Services.Localization;
using Nop.Web.Framework.Factories;
using System;

namespace Nop.Plugin.Misc.AppointmentBooking.Areas.Admin.Factories
{
    public interface IAppointmentBranchModelFactory
    {
        AppointmentBranchModel PrepareAppointmentBranchModel(AppointmentBranchModel model,
            AppointmentBranch appointmentBranch);
    }

    public class AppointmentBranchModelFactory : IAppointmentBranchModelFactory
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        #endregion

        #region Ctor
        public AppointmentBranchModelFactory(ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory)
        {
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prepare appointment branch model
        /// </summary>
        /// <param name="model">AppointmentBranchModel</param>
        /// <param name="appointmentBranch">appointmentBranch</param>
        /// <returns>AppointmentBranchModel</returns>
        public virtual AppointmentBranchModel PrepareAppointmentBranchModel(AppointmentBranchModel model,
            AppointmentBranch appointmentBranch)
        {
            Action<AppointmentBranchLocalizedModel, int> localizedModelConfiguration = null;

            if (appointmentBranch != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new AppointmentBranchModel()
                    {
                        Id = appointmentBranch.Id,
                        BranchId = appointmentBranch.BranchId,
                        Name = appointmentBranch.Name,
                        Longitude = appointmentBranch.Longitude,
                        Latitude = appointmentBranch.Latitude
                    };
                }
                
                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(appointmentBranch, entity => entity.Name, languageId, false, false);
                };
            }

            //prepare localized models
            model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);
            
            return model;
        }
        #endregion
    }
}
