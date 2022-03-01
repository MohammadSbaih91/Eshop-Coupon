using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Data.Extensions;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Localization;

namespace Nop.Services.Customers
{
    public partial interface ICustomerService
    {
        Customer GetCustomerByEmailOrUser(string username, string Email);
    }

    public partial class CustomerService
    {
        public virtual Customer GetCustomerByEmailOrUser(string username, string email)
        {
            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(email))
                return null;

            var customer = _customerRepository.Table.FirstOrDefault(p => (email != null && p.Email == email) || (username != null && p.Username == username));

            return customer;
        }
    }
}
