using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Messages;
using Nop.Services.Events;

namespace Nop.Services.Messages
{
    public class NewsLetterSubscriptionTypeService : INewsLetterSubscriptionTypeService
    {
        #region Fields
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<NewsLetterSubscriptionType> _newsLetterSubscriptionTypeRepository;
        #endregion

        #region Ctor
        public NewsLetterSubscriptionTypeService(IEventPublisher eventPublisher,
            IRepository<NewsLetterSubscriptionType> newsLetterSubscriptionTypeRepository)
        {
            _eventPublisher = eventPublisher;
            _newsLetterSubscriptionTypeRepository = newsLetterSubscriptionTypeRepository;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a NewsLetterSubscriptionType
        /// </summary>
        /// <param name="NewsLetterSubscriptionTypeId">The NewsLetterSubscriptionType identifier</param>
        /// <returns>newsLetterSubscriptionType</returns>
        public virtual NewsLetterSubscriptionType GetNewsLetterSubscriptionTypeById(int newsLetterSubscriptionTypeId)
        {
            if (newsLetterSubscriptionTypeId == 0)
                return null;

            return _newsLetterSubscriptionTypeRepository.GetById(newsLetterSubscriptionTypeId);
        }

        /// <summary>
        /// Gets newsLetterSubscriptionType
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>newsLetterSubscriptionType</returns>
        public virtual IPagedList<NewsLetterSubscriptionType> GetNewsLetterSubscriptionTypes(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from sa in _newsLetterSubscriptionTypeRepository.Table
                        orderby sa.DisplayOrder, sa.Id
                        select sa;
            var newsLetterSubscriptionTypes = new PagedList<NewsLetterSubscriptionType>(query, pageIndex, pageSize);
            return newsLetterSubscriptionTypes;
        }

        /// <summary>
        /// Deletes a newsLetterSubscriptionType
        /// </summary>
        /// <param name="newsLetterSubscriptionType">The newsLetterSubscriptionType</param>
        public virtual void DeleteNewsLetterSubscriptionType(NewsLetterSubscriptionType newsLetterSubscriptionType)
        {
            if (newsLetterSubscriptionType == null)
                throw new ArgumentNullException(nameof(newsLetterSubscriptionType));

            _newsLetterSubscriptionTypeRepository.Delete(newsLetterSubscriptionType);

            //event notification
            _eventPublisher.EntityDeleted(newsLetterSubscriptionType);
        }

        /// <summary>
        /// Inserts a newsLetterSubscriptionType
        /// </summary>
        /// <param name="newsLetterSubscriptionType">The newsLetterSubscriptionType</param>
        public virtual void InsertNewsLetterSubscriptionType(NewsLetterSubscriptionType newsLetterSubscriptionType)
        {
            if (newsLetterSubscriptionType == null)
                throw new ArgumentNullException(nameof(newsLetterSubscriptionType));

            _newsLetterSubscriptionTypeRepository.Insert(newsLetterSubscriptionType);

            //event notification
            _eventPublisher.EntityInserted(newsLetterSubscriptionType);
        }

        /// <summary>
        /// Updates newsLetterSubscriptionType
        /// </summary>
        /// <param name="newsLetterSubscriptionType">The newsLetterSubscriptionType</param>
        public virtual void UpdateNewsLetterSubscriptionType(NewsLetterSubscriptionType newsLetterSubscriptionType)
        {
            if (newsLetterSubscriptionType == null)
                throw new ArgumentNullException(nameof(newsLetterSubscriptionType));

            _newsLetterSubscriptionTypeRepository.Update(newsLetterSubscriptionType);

            //event notification
            _eventPublisher.EntityUpdated(newsLetterSubscriptionType);
        }
        #endregion
    }
}
