using System;


namespace Nop.Core.Customization.Domain.Orders
{
    public class CrReport
    {
        public int Id { get; set; }
        public string CustomOrderNumber { get; set; }
        public int OrderStatusId { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal OrderTotal { get; set; }
        public string ProductName { get; set; }
        public string VendorNames { get; set; }
        public string CustomerId { get; set; }
    }
}
