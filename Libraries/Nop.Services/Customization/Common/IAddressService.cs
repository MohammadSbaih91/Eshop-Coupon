using System.Collections.Generic;
using Nop.Core.Domain.Common;

namespace Nop.Services.Common
{
    /// <summary>
    /// Address service interface
    /// </summary>
    public partial interface IAddressService
    {
        /// <summary>
        /// Find an address
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="phoneNumber">Phone number</param>
        /// <param name="email">Email</param>
        /// <param name="faxNumber">Fax number</param>
        /// <param name="company">Company</param>
        /// <param name="address1">Address 1</param>
        /// <param name="address2">Address 2</param>
        /// <param name="city">City</param>
        /// <param name="county">County</param>
        /// <param name="stateProvinceId">State/province identifier</param>
        /// <param name="zipPostalCode">Zip postal code</param>
        /// <param name="countryId">Country identifier</param>
        /// <param name="customAttributes">Custom address attributes (XML format)</param>
        /// <param name="Civility">Civility</param>
        /// <param name="Nationality">Nationality</param>
        /// <param name="NationalityType">NationalityType</param>
        /// <param name="identityCardOrPassport">identityCardOrPassport</param>
        /// <returns>Address</returns>
        Address FindAddress(List<Address> source, string firstName, string lastName, string phoneNumber, string email,
            string faxNumber, string company, string address1, string address2, string city, string county,
            int? stateProvinceId,
            string zipPostalCode, int? countryId, string customAttributes, Civility civility, Nationality nationality,
            NationalityType nationalityType, string identityCardOrPassport);
    }
}