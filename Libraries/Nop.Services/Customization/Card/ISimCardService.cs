using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using System.IO;

namespace Nop.Services.Card
{
    public interface ISimCardService
    {
        SimCard GetSimCardById(int simCardId);
        SimCard GetSimCardByNumber(string cardNumber);
        IPagedList<SimCard> GetSimCards(string cardNumber = "", string group = "", int productId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<SimCard> GetSimCardList();
        void DeleteSimCard(SimCard simCard);
        void InsertSimCard(SimCard simCard);
        void InsertSimCards(IList<SimCard> simCards);
        void UpdateSimCard(SimCard simCard);
        IList<SimCard> GetSimCardListByProductId(int productId);

        #region Product Simcard Mapping
        IList<ProductSimcardMapping> GetProductSimcardByProductId(int productId);
        ProductSimcardMapping GetProductSimCardById(int productSimCardId);
        void InsertProductsimcard(ProductSimcardMapping productSimcardMapping);
        void DeleteProductsimcard(ProductSimcardMapping productSimcardMapping);
        IPagedList<SimCard> GetSimCardsList(string cardNumber = "", string group = "", int productId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
        #endregion
    }
}
