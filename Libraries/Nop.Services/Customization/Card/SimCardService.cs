using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Events;
using OfficeOpenXml;

namespace Nop.Services.Card
{
    public class SimCardService : ISimCardService
    {
        #region Fields
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<SimCard> _simCardRepository;
        //private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<ProductSimcardMapping> _productsimcardRepository;
        #endregion

        #region Ctor

        public SimCardService(IEventPublisher eventPublisher,
            IRepository<SimCard> simCardRepository,
            //IRepository<OrderItem> orderItemRepository,
            IRepository<ProductSimcardMapping> productsimcardRepository)
        {
            _eventPublisher = eventPublisher;
            _simCardRepository = simCardRepository;
            //_orderItemRepository = orderItemRepository;
            _productsimcardRepository = productsimcardRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a sim card
        /// </summary>
        /// <param name="simCardId">The simcard identifier</param>
        /// <returns>SimCard</returns>
        public virtual SimCard GetSimCardById(int simCardId)
        {
            if (simCardId == 0)
                return null;

            return _simCardRepository.GetById(simCardId);
        }

        /// <summary>
        /// Gets a sim card
        /// </summary>
        /// <param name="cardNumber">The simcard number</param>
        /// <returns>SimCard</returns>
        public virtual SimCard GetSimCardByNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return null;

            return _simCardRepository.Table.Where(p => p.CardNumber == cardNumber).FirstOrDefault();
        }

        /// <summary>
        /// Gets SimCards
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>SimCard</returns>
        public virtual IPagedList<SimCard> GetSimCards(string cardNumber = "", string group = "",int productId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var productSimCardMapping = _productsimcardRepository.Table.Where(x => x.ProductId == productId).Select(x => x.SimCardId).ToList();

            var query = _simCardRepository.Table;

            if (!string.IsNullOrEmpty(cardNumber))
                query = query.Where(p => p.CardNumber.Contains(cardNumber));

            if (!string.IsNullOrEmpty(group))
                query = query.Where(p => p.Group.Contains(group));

            if (productSimCardMapping.Count > 0)
                query = query.Where(p => !productSimCardMapping.Contains(p.Id));

            query = from sa in query
                    orderby sa.DisplayOrder, sa.Id
                    select sa;
            return new PagedList<SimCard>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets SimCards Front
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>SimCard</returns>
        public virtual IList<SimCard> GetSimCardList()
        {
            var query = _simCardRepository.Table.Where(s=> !s.IsSale).OrderBy(s=>s.DisplayOrder);
            //var orderItem = _orderItemRepository.Table;

            //query = from item in query
            //        where !(from oi in orderItem
            //                select oi.SimCardId).Contains(item.Id)
            //        orderby item.DisplayOrder, item.Id
            //        select item;

            return new List<SimCard>(query);
        }

        public virtual IList<SimCard> GetSimCardListByProductId(int productId)
        {
            var query = _simCardRepository.Table.Where(p=> !p.IsSale);
            //var orderItem = _orderItemRepository.Table;
            var simcardmapping = _productsimcardRepository.Table;

            query = from item in query
                    join sm in simcardmapping on item.Id equals sm.SimCardId
                    where sm.ProductId == productId
                    orderby item.DisplayOrder, item.Id
                    select item;

            return new List<SimCard>(query);
        }

        /// <summary>
        /// Deletes a SimCard
        /// </summary>
        /// <param name="SimCard">The SimCard</param>
        public virtual void DeleteSimCard(SimCard simCard)
        {
            if (simCard == null)
                throw new ArgumentNullException(nameof(simCard));

            _simCardRepository.Delete(simCard);

            //event notification
            _eventPublisher.EntityDeleted(simCard);
        }

        /// <summary>
        /// Inserts a SimCard
        /// </summary>
        /// <param name="SimCard">SimCard</param>
        public virtual void InsertSimCard(SimCard simCard)
        {
            if (simCard == null)
                throw new ArgumentNullException(nameof(simCard));

            _simCardRepository.Insert(simCard);

            //event notification
            _eventPublisher.EntityInserted(simCard);
        }

        public virtual void InsertSimCards(IList<SimCard> simCards)
        {
            _simCardRepository.Insert(simCards);
        }

        /// <summary>
        /// Updates the SimCard
        /// </summary>
        /// <param name="SimCard">SimCard</param>
        public virtual void UpdateSimCard(SimCard simCard)
        {
            if (simCard == null)
                throw new ArgumentNullException(nameof(simCard));

            _simCardRepository.Update(simCard);

            //event notification
            _eventPublisher.EntityUpdated(simCard);
        }
        #endregion

        #region Product Simcard Mapping
        public IList<ProductSimcardMapping> GetProductSimcardByProductId(int productId)
        {
            if (productId == 0)
                return null;

            var result = from ps in _productsimcardRepository.Table
                         where ps.ProductId == productId
                         select ps;
            return result.ToList();
        }

        public ProductSimcardMapping GetProductSimCardById(int id)
        {
            if (id == 0)
                return null;

            return _productsimcardRepository.GetById(id);
        }

        public void InsertProductsimcard(ProductSimcardMapping productSimcardMapping)
        {
            _productsimcardRepository.Insert(productSimcardMapping);
        }
        public void DeleteProductsimcard(ProductSimcardMapping productSimcardMapping)
        {
            _productsimcardRepository.Delete(productSimcardMapping);
        }

        /// <summary>
        /// Gets SimCards List
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>SimCard</returns>
        public virtual IPagedList<SimCard> GetSimCardsList(string cardNumber = "", string group = "", int productId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var productSimCardMapping = _productsimcardRepository.Table.Where(x => x.ProductId == productId).Select(x => x.SimCardId).ToList();

            var query = _simCardRepository.Table;

            if (!string.IsNullOrEmpty(cardNumber))
                query = query.Where(p => p.CardNumber.Contains(cardNumber));

            if (!string.IsNullOrEmpty(group))
                query = query.Where(p => p.Group.Contains(group));

            if (productSimCardMapping.Count > 0)
                query = query.Where(p => !productSimCardMapping.Contains(p.Id));

            query = from sa in query
                    where !sa.IsSale
                    orderby sa.DisplayOrder, sa.Id
                    select sa;
            return new PagedList<SimCard>(query, pageIndex, pageSize);
        }
        #endregion
    }
}
