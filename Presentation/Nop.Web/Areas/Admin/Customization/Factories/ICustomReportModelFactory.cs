using Nop.Core.Customization.Domain.Orders;
using Nop.Web.Areas.Admin.Customization.Models.Report;
using System;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Customization.Factories
{
    public interface ICustomReportModelFactory
    {
        CustomReportSearchModel PrepareCustomReportsSearchModel(CustomReportSearchModel searchModel);

        /// <summary>
        /// Prepare custom report
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        CustomReportListModel PrepareCustomReportListModel(CustomReportSearchModel searchModel);

        List<CustomReportModel> PrepareCustomReportModel(out int totalCount, DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int orderStatus = 0, int productId = 0, int pageIndex = 0, int pageSize = int.MaxValue-1);

        /// <summary>
        /// Export orders to XLSX
        /// </summary>
        /// <param name="orders">Orders</param>
        byte[] ExportCustomizeReportToXlsx(IList<CustomReportModel> reportModels);
    }
}
