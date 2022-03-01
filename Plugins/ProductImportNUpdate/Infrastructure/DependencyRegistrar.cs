using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Services.Tasks;
using ProductImportNUpdate.Services;

namespace ProductImportNUpdate.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ImportServiceCsv>().As<IImportService>().InstancePerLifetimeScope();
            builder.RegisterType<AutoImportNUpdateTask>().As<IScheduleTask>().InstancePerLifetimeScope();
            
        }

        public int Order => 1;
    }
}