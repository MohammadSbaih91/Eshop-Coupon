using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Catalog
{
    public partial interface IPriceFormatter
    {
        string FormatOldPrice(decimal price);
    }

    public partial class PriceFormatter
    {
        public virtual string FormatOldPrice(decimal price)
        {
            return price.ToString(_localizationService.GetResource("Common.OldPriceFormate"));
        }
    }
}
