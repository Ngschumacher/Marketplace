using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Misc.Vendor.Services.Catalog;

namespace Nop.Plugin.Misc.Vendor.Infastructure
{
	public class DependencyRegistrar : IDependencyRegistrar
	{
		public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
		{
			builder.RegisterType<CustomCategoryService>().As<ICustomCategoryService>().InstancePerLifetimeScope();
		}

		public int Order
		{
			get { return 100; }
		}
	}
}
