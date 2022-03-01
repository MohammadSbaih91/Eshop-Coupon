namespace Nop.Plugin.Shipping.Aramex.Services
{
    public interface ITrackingService
    {
        string GetOrderTrackingNumber(int orderId, string emailId, out string errorMessage);
    }
}
