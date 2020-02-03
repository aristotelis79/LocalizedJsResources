using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Misc.Localized.Javascript.Data;
using Nop.Plugin.Misc.Localized.Javascript.Domain;
using Nop.Plugin.Misc.Localized.Javascript.Services;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.Localized.Javascript.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<JavascriptResourceService>().As<IJavascriptResourceService>().InstancePerLifetimeScope();

            //data context
            this.RegisterPluginDataContext<JavaScriptResourceObjectContext>(builder, "nop_object_context_javascript_resource");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<JavaScriptResourceRecord>>()
                .As<IRepository<JavaScriptResourceRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_javascript_resource"))
                .InstancePerLifetimeScope();
        }

        public int Order => 10;
    }
}