﻿#region Copyright (c) Lokad 2009-2012
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using Autofac;
using Lokad.Cloud.Storage;
using Lokad.Cloud.Storage.InMemory;

namespace Lokad.Cloud.Autofac.Storage
{
    /// <summary>
    /// IoC Module that provides simple stub in-memory providers without any diagostics attached:
    /// - CloudStorageProviders
    /// - IBlobStorageProvider
    /// - IQueueStorageProvider
    /// - ITableStorageProvider
    /// </summary>
    public sealed class StubStorageModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => CloudStorage.ForInMemoryStorage().BuildStorageProviders());

            builder.Register(c => new MemoryBlobStorageProvider()).As<IBlobStorageProvider>();
            builder.Register(c => new MemoryQueueStorageProvider()).As<IQueueStorageProvider>();
            builder.Register(c => new MemoryTableStorageProvider()).As<ITableStorageProvider>();

            builder.Register(c => new Diagnostics.NeutralLogStorage { BlobStorage = new MemoryBlobStorageProvider() });
        }
    }
}
