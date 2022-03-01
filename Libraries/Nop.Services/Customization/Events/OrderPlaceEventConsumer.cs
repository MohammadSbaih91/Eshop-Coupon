using Nop.Core.Domain.Orders;
using Nop.Services.Card;

namespace Nop.Services.Events
{
    public class OrderPlaceEventConsumer : IConsumer<OrderPlacedEvent>
    {

        #region Fields
        private readonly ISimCardService _simCardService;
        #endregion

        #region Ctor
        public OrderPlaceEventConsumer(ISimCardService simCardService)
        {   
            _simCardService = simCardService;
        }
        #endregion

        #region Methods
        public void HandleEvent(OrderPlacedEvent eventMessage)
        {
            var order = eventMessage.Order;
            if (order != null)
            {
                if (order.OrderItems != null && order.OrderItems.Count > 0)
                {
                    foreach (var item in order.OrderItems)
                    {
                        var simcard = _simCardService.GetSimCardById(item.SimCardId);
                        if (simcard != null)
                        {
                            simcard.IsSale = true;
                            _simCardService.UpdateSimCard(simcard);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
