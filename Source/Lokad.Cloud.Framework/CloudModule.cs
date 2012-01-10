﻿#region Copyright (c) Lokad 2009-2011
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using Autofac;

namespace Lokad.Cloud
{
    /// <summary>
    /// IoC module that registers all usually required components, including
    /// storage providers, management & provisioning and diagnostics/logging.
    /// It is recommended to load this module even when only using the storage (O/C mapping) providers.
    /// Expects the <see cref="CloudConfigurationModule"/> (or the mock module) to be registered as well.
    /// </summary>
    /// <remarks>
    /// When only using the storage (O/C mapping) toolkit standalone it is easier
    /// to let the <see cref="Standalone"/> factory create the storage providers on demand.
    /// </remarks>
    /// <seealso cref="CloudConfigurationModule"/>
    /// <seealso cref="Standalone"/>
    public sealed class CloudModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new Storage.Azure.StorageModule());
            builder.RegisterModule(new Diagnostics.DiagnosticsModule());
            builder.RegisterModule(new Management.ManagementModule());

            builder.RegisterType<Jobs.JobManager>();
            builder.RegisterType<ServiceFabric.RuntimeFinalizer>().As<IRuntimeFinalizer>().InstancePerLifetimeScope();

            // NOTE: Guid is not very nice, but this will be replaced anyway once fully ported to AppHost
            builder.Register(c => new CloudEnvironment(c.Resolve<Management.IProvisioningProvider>(), Guid.NewGuid().ToString("N"))).As<ICloudEnvironment>().SingleInstance();
        }
    }
}
