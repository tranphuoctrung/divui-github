using System;
using Autofac;
using Autofac.Core;
using Nop.Admin.Controllers;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Services.Divui.Catalog;
using Nop.Services.Catalog;

namespace Nop.Admin.Infrastructure
{
    public class DvDependencyRegistrar : IDependencyRegistrar
    {
        public int Order
        {
            get { return 0; }
        }

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ProductOptionService>().As<IProductOptionService>().InstancePerLifetimeScope();
            builder.RegisterType<PriceSetupService>().As<IPriceSetupService>().InstancePerLifetimeScope();
            builder.RegisterType<AvailabilitySetupService>().As<IAvailabilitySetupService>().InstancePerLifetimeScope();

        }
    }
}