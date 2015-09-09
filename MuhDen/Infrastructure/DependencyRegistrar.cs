using Autofac;
using Muh.Core.Infrastructure;
using Muh.Core.Infrastructure.DependencyManagement;
using MuhDen.Infrastructure.Installation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MuhDen.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            //we cache presentation models between requests
           

            //installation localization service
            builder.RegisterType<InstallationLocalizationService>().As<IInstallationLocalizationService>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 2; }
        }
    }

}