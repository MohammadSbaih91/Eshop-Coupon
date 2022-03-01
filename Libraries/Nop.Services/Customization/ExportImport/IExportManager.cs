using Nop.Core.Domain.Orders;
using System.Collections.Generic;

namespace Nop.Services.ExportImport
{
    public partial interface IExportManager
    {
        /// <summary>
        /// Export orders to XLSX
        /// </summary>
        /// <param name="orders">Orders</param>
        byte[] ExportCustomizeReportToXlsx(IList<Order> orders);
    }
}
