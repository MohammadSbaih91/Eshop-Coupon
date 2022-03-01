using Nop.Core;
using Nop.Core.Domain.Messages;

namespace Nop.Services.Messages
{
    public interface INewsLetterSubscriptionTypeService
    {
        NewsLetterSubscriptionType GetNewsLetterSubscriptionTypeById(int newsLetterSubscriptionTypeId);

        IPagedList<NewsLetterSubscriptionType> GetNewsLetterSubscriptionTypes(int pageIndex = 0, int pageSize = int.MaxValue);
        void DeleteNewsLetterSubscriptionType(NewsLetterSubscriptionType newsLetterSubscriptionType);

        void InsertNewsLetterSubscriptionType(NewsLetterSubscriptionType newsLetterSubscriptionType);

        void UpdateNewsLetterSubscriptionType(NewsLetterSubscriptionType newsLetterSubscriptionType);
    }
}
