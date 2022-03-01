
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Events;

namespace SevenSpikes.Nop.Plugins.AjaxCart.Infrastructure.Cache
{
  public class AjaxCartModelCacheEventConsumer : IConsumer<EntityInsertedEvent<Product>>
  {
    private IStaticCacheManager CacheManager { get; set; }

    public AjaxCartModelCacheEventConsumer() => CacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();

    public void HandleEvent(EntityInsertedEvent<Product> eventMessage) => CacheManager.RemoveByPattern("nop.pres.7spikes.ajaxcart");
  }
}
