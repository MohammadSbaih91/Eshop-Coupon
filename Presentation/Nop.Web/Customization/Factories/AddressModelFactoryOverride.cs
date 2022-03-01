using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Models.Common;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the address model factory
    /// </summary>
    public partial class AddressModelFactoryOverride : AddressModelFactory
    {
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        public AddressModelFactoryOverride(AddressSettings addressSettings, IWorkContext workContext, IStoreContext storeContext,
            IAddressAttributeFormatter addressAttributeFormatter, IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService, IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService, IStateProvinceService stateProvinceService)
            : base(addressSettings, addressAttributeFormatter, addressAttributeParser, addressAttributeService,
                genericAttributeService, localizationService, stateProvinceService)
        {
            _localizationService = localizationService;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        #region Methods

        /// <summary>
        /// Prepare address model
        /// </summary>
        /// <param name="model">Address model</param>
        /// <param name="address">Address entity</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="addressSettings">Address settings</param>
        /// <param name="loadCountries">Countries loading function; pass null if countries do not need to load</param>
        /// <param name="prePopulateWithCustomerFields">Whether to populate model properties with the customer fields (used with the customer entity)</param>
        /// <param name="customer">Customer entity; required if prePopulateWithCustomerFields is true</param>
        /// <param name="overrideAttributesXml">Overridden address attributes in XML format; pass null to use CustomAttributes of the address entity</param>
        public override void PrepareAddressModel(AddressModel model,
            Address address, bool excludeProperties,
            AddressSettings addressSettings,
            Func<IList<Country>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            string overrideAttributesXml = "")
        {
            base.PrepareAddressModel(model, address, excludeProperties, addressSettings, loadCountries,
                prePopulateWithCustomerFields, customer, overrideAttributesXml);
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
               .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
               .LimitPerStore(_storeContext.CurrentStore.Id)
               .ToList();

            model.Civility = address?.Civility ?? Civility.Mr;
            model.Nationality = address?.Nationality ?? model.Nationality ;
            model.NationalityType = address?.NationalityType ?? model.NationalityType ;
            model.IdentityCardOrPassport = address?.IdentityCardOrPassport;
            model.EmailConfirm = model.EmailConfirm ?? model.Email;
            model.StudentID = address?.StudentID;
            model.UploadID = address?.UploadID;
            model.UploadStudentID = address?.UploadStudentID;
            model.IsStudentIdNeeded = cart.Any(x => x.Product.IsStudentIdNeeded);

            var nationalities = Enum.GetValues(typeof(Nationality)).Cast<Nationality>()
                .Select(x => new SelectListItem
                {
                    Value = ((int) x).ToString(), 
                    Text = _localizationService.GetResource($"Address.Fields.Nationality.{x}") , 
                    Selected = address?.Nationality == x
                }).ToList();
            nationalities.Insert(0,
                new SelectListItem
                {
                    Value = "", Text = _localizationService.GetResource("Address.Fields.Nationality.Select")
                });
            
            model.Nationalities = nationalities;

            var nationalityTypes = Enum.GetValues(typeof(NationalityType)).Cast<NationalityType>()
                .Select(x => new SelectListItem
                {
                    Value = ((int) x).ToString(), 
                    Text = _localizationService.GetResource($"Address.Fields.NationalityType.{x}") , 
                    Selected = address?.NationalityType == x
                }).ToList();
            nationalityTypes.Insert(0,
                new SelectListItem
                {
                   Value = "", Text = _localizationService.GetResource("Address.Fields.NationalityType.Select")
                });
            
            model.NationalityTypes = nationalityTypes;

            if (!addressSettings.CountryEnabled || loadCountries == null
                                                || !model.AvailableCountries.Any(x => x.Value == "0" && x.Selected)
                                                && model.AvailableCountries.Any(x => x.Selected))
                return;

            var jordanCountry = loadCountries()?.FirstOrDefault(c => c.NumericIsoCode == 400);
            var jordanId = jordanCountry?.Id ?? 0;
            var jordan = model.AvailableCountries.FirstOrDefault(ac =>
                ac.Value.Equals(jordanId.ToString(), StringComparison.Ordinal));
            if (jordan != null)
            {
                jordan.Selected = true;
            }

            model.County = address?.County ?? jordanCountry?.Name;
        }

        #endregion
    }
}