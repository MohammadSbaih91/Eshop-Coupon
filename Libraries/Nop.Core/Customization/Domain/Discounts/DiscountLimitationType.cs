using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Discounts
{
    public enum DiscountLimitationType
    {
        /// <summary>
        /// None
        /// </summary>
        Unlimited = 0,

        /// <summary>
        /// N Times Only
        /// </summary>
        NTimesOnly = 15,
    }
}
