#region Copyright (c) Lokad 2010-2011
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Lokad.Cloud.Diagnostics;
using Lokad.Cloud.Mock;
using Lokad.Cloud.ServiceFabric;
using Lokad.Cloud.Storage;
using NUnit.Framework;

namespace Lokad.Cloud.Test.Services
{
    [TestFixture]
    public class QueueServiceMultipleTests
    {
        [Test]
        public void SquareServiceTest()
        {
            var storage = CloudStorage.ForInMemoryStorage().BuildStorageProviders();
            var service = new SquareQueueService { Storage = storage, Environment = new MockEnvironment(), Log = NullLog.Instance};

            const string containerName = "mockcontainer";

            //filling blobs to be processed.
            for (int i = 0; i < 10; i++)
            {
                storage.BlobStorage.PutBlob(containerName, "blob" + i, (double)i);
            }

            var squareMessage = new SquareMessage
            {
                ContainerName = containerName,
                Expiration = DateTimeOffset.UtcNow + new TimeSpan(10, 0, 0, 0),
                IsStart = true
            };

            var queueName = TypeMapper.GetStorageName(typeof(SquareMessage));
            storage.QueueStorage.Put(queueName, squareMessage);

            for (int i = 0; i < 11; i++)
            {
                service.StartService();
            }

            var sum = storage.BlobStorage.ListBlobs<double>(containerName).Sum();

            //0*0+1*1+2*2+3*3+...+9*9 = 285
            Assert.AreEqual(285, sum, "result is different from expected.");
        }

        [Test]
        public void SquareServiceTestPulls32MessagesOffOfQueue()
        {
            var storage = CloudStorage.ForInMemoryStorage().BuildStorageProviders();
            var service = new SquareQueueServiceRange { Storage = storage, Environment = new MockEnvironment(), Log = NullLog.Instance };

            const string containerName = "mockcontainer";
            const string blobname = "blobname";

            List<SquareMessage> messages = new List<SquareMessage>();
            for (int i = 0; i < 100; i++)
            {
                messages.Add(
                    new SquareMessage
                        {
                            ContainerName = containerName,
                            Expiration = DateTimeOffset.UtcNow + new TimeSpan(10, 0, 0, 0),
                            IsStart = true,
                            BlobName = blobname
                        }
                    );
            }

            var queueName = TypeMapper.GetStorageName(typeof(SquareMessage));
            storage.QueueStorage.PutRange(queueName,messages);

            service.StartService();

            Maybe<int> count = storage.BlobStorage.GetBlob<int>(containerName, blobname);
            int queueCount = storage.QueueStorage.GetApproximateCount(queueName);

            Assert.AreEqual(32, count.Value);
            Assert.AreEqual(68, queueCount);
        }

        [QueueServiceSettings(AutoStart = true,
            Description = "Write count of messages to blob")]
        private class SquareQueueServiceRange : QueueServiceMultiple<SquareMessage>
        {
            public void StartService()
            {
                StartImpl();
            }
            protected override void StartRange(IEnumerable<SquareMessage> messages)
            {
                int count = messages.Count();
                Blobs.PutBlob(messages.First().ContainerName, messages.First().BlobName, count);
            }
        }

        [QueueServiceSettings(AutoStart = true,
        Description = "multiply numbers by themselves.")]
        class SquareQueueService : QueueServiceMultiple<SquareMessage>
        {
            public void StartService()
            {
                StartImpl();
            }

            protected override void StartRange(IEnumerable<SquareMessage> messages)
            {
                  foreach (var message in messages)
                  {
                      if (message.IsStart)
                      {
                          var counterName = TemporaryBlobName<decimal>.GetNew(message.Expiration);
                          var counter = new BlobCounter(Blobs, counterName);
                          counter.Reset(BlobCounter.Aleph);

                          var blobNames = Blobs.ListBlobNames(message.ContainerName).ToList();

                          foreach (var blobName in blobNames)
                          {
                              Put(new SquareMessage
                              {
                                  BlobName = blobName,
                                  ContainerName = message.ContainerName,
                                  IsStart = false,
                                  BlobCounter = counterName
                              });
                          }

                          // dealing with rare race condition
                          if (0m >= counter.Increment(-BlobCounter.Aleph + blobNames.Count))
                          {
                              Finish(counter);
                          }

                      }
                      else
                      {
                          var value = Blobs.GetBlob<double>(message.ContainerName, message.BlobName).Value;
                          Blobs.PutBlob(message.ContainerName, message.BlobName, value * value);

                          var counter = new BlobCounter(Blobs, message.BlobCounter);
                          if (0m >= counter.Increment(-1))
                          {
                              Finish(counter);
                          }
                      }
                  }
            }

            void Finish(BlobCounter counter)
            {
                counter.Delete();
            }
        }
    }
}
