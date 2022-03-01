using Nop.Web.Areas.Admin.Models.EmployeeOrder;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Web.Areas.Admin.Factories
{
    public interface IEmployeeOrderModelFactory
    {
        EmployeeOrderSearchModel PrepareCustomReportsSearchModel(EmployeeOrderSearchModel searchModel);

        OrderListModel PrepareOrderListModel(EmployeeOrderSearchModel searchModel);
    }
}
