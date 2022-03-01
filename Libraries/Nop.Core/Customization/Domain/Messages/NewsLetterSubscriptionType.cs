using Nop.Core.Domain.Localization;

namespace Nop.Core.Domain.Messages
{
    public class NewsLetterSubscriptionType : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
