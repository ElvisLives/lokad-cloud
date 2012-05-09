using System;
using Lokad.Cloud.Storage;

namespace Lokad.Cloud.Test.Services
{
    [Serializable]
    internal class SquareMessage
    {
        public bool IsStart { get; set; }

        public DateTimeOffset Expiration { get; set; }

        public string ContainerName { get; set; }

        public string BlobName { get; set; }

        public TemporaryBlobName<decimal> BlobCounter { get; set; }
    }
}