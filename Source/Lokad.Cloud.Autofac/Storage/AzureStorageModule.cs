﻿#region Copyright (c) Lokad 2009-2012
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System.Net;
using Autofac;
using Lokad.Cloud.Storage;
using Lokad.Cloud.Storage.Instrumentation;
using Microsoft.WindowsAzure;

namespace Lokad.Cloud.Autofac.Storage
{
    /// <summary>
    /// IoC Module that provides storage providers linked to Windows Azure storage:
    /// - CloudStorageProviders
    /// - IBlobStorageProvider
    /// - IQueueStorageProvider
    /// - ITableStorageProvider
    /// 
    /// Expected external registrations:
    /// - Microsoft.WindowsAzure.CloudStorageAccount
    /// </summary>
    public sealed class AzureStorageModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => CloudStorage
                .ForAzureAccount(Patch(c.Resolve<CloudStorageAccount>()))
                .WithDataSerializer(c.Resolve<IDataSerializer>())
                .WithObserver(c.ResolveOptional<ICloudStorageObserver>())
                .WithRuntimeFinalizer(c.ResolveOptional<IRuntimeFinalizer>())
                .BuildStorageProviders());

            builder.Register(c => CloudStorage
                .ForAzureAccount(Patch(c.Resolve<CloudStorageAccount>()))
                .WithDataSerializer(c.Resolve<IDataSerializer>())
                .WithObserver(c.ResolveOptional<ICloudStorageObserver>())
                .WithRuntimeFinalizer(c.ResolveOptional<IRuntimeFinalizer>())
                .BuildBlobStorage());

            builder.Register(c => CloudStorage
                .ForAzureAccount(Patch(c.Resolve<CloudStorageAccount>()))
                .WithDataSerializer(c.Resolve<IDataSerializer>())
                .WithObserver(c.ResolveOptional<ICloudStorageObserver>())
                .WithRuntimeFinalizer(c.ResolveOptional<IRuntimeFinalizer>())
                .BuildQueueStorage());

            builder.Register(c => CloudStorage
                .ForAzureAccount(Patch(c.Resolve<CloudStorageAccount>()))
                .WithDataSerializer(c.Resolve<IDataSerializer>())
                .WithObserver(c.ResolveOptional<ICloudStorageObserver>())
                .WithRuntimeFinalizer(c.ResolveOptional<IRuntimeFinalizer>())
                .BuildTableStorage());

            builder.Register(c => new Diagnostics.NeutralLogStorage
                {
                    BlobStorage = CloudStorage.ForAzureAccount(Patch(c.Resolve<CloudStorageAccount>())).WithDataSerializer(new CloudFormatter()).BuildBlobStorage()
                });
        }

        private CloudStorageAccount Patch(CloudStorageAccount account)
        {
            ServicePointManager.FindServicePoint(account.BlobEndpoint).UseNagleAlgorithm = false;
            ServicePointManager.FindServicePoint(account.TableEndpoint).UseNagleAlgorithm = false;
            ServicePointManager.FindServicePoint(account.QueueEndpoint).UseNagleAlgorithm = false;
            return account;
        }
    }
}