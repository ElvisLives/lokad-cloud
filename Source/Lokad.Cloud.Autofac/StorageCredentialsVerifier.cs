﻿#region Copyright (c) Lokad 2009
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Lokad.Cloud.Storage;

namespace Lokad.Cloud.Autofac
{
    /// <summary>
    /// Verifies that storage credentials are correct and allow access to blob and queue storage.
    /// </summary>
    public class StorageCredentialsVerifier
    {
        private readonly IBlobStorageProvider _storage;

        /// <summary>
        /// Initializes a new instance of the StorageCredentialsVerifier class.
        /// </summary>
        /// <param name="container">The IoC container.</param>
        public StorageCredentialsVerifier(IContainer container)
        {
            try
            {
                _storage = container.Resolve<IBlobStorageProvider>();
            }
            catch(ComponentNotRegisteredException) { }
            catch(DependencyResolutionException) { }
        }

        /// <summary>
        /// Verifies the storage credentials.
        /// </summary>
        /// <returns><c>true</c> if the credentials are correct, <c>false</c> otherwise.</returns>
        public bool VerifyCredentials()
        {
            if(_storage == null) return false;

            try
            {
                var containers = _storage.ListContainers();

                // It is necssary to enumerate in order to actually send the request
                foreach (var c in containers)
                {
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}