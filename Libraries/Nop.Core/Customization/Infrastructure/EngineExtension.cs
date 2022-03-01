using Autofac;
using Nop.Core.Infrastructure;

namespace Nop.Core.Infrastructure
{
    public static class EngineExtension
    {
        public static T ResolveNamed<T>(this IEngine engine, string name) where T : class 
            => EngineContext.Current.Resolve<IComponentContext>().ResolveNamed<T>(name);
    }
}
