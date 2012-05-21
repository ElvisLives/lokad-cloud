﻿#region Copyright (c) Lokad 2009-2011
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Lokad.Cloud.ServiceFabric
{
    /// <summary>Strongly-type queue service that handles the max messages off an Azure queue (32 messages at once) (inheritors are instantiated by
    /// reflection on the cloud).</summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <remarks>
    /// <para>The implementation is not constrained by the 8kb limit for <c>T</c> instances.
    /// If the instances are larger, the framework will wrap them into the cloud storage.</para>
    /// <para>Whenever possible, we suggest to design the service logic to be idempotent
    /// in order to make the service reliable and ultimately consistent.</para>
    /// <para>A empty constructor is needed for instantiation through reflection.</para>
    /// </remarks>
    public abstract class QueueServiceMultiple<T> : CloudService
        where T : class
    {
        const int MaxAzureQueuePullSize = 32;
        readonly string _queueName;
        readonly string _serviceName;
        readonly TimeSpan _visibilityTimeout;
        readonly int _maxProcessingTrials;

        /// <summary>Name of the queue associated to the service.</summary>
        public override string Name
        {
            get { return _serviceName; }
        }

        /// <summary>Default constructor</summary>
        protected QueueServiceMultiple()
        {
            var settings = GetType().GetCustomAttributes(typeof(QueueServiceSettingsAttribute), true)
                                    .FirstOrDefault() as QueueServiceSettingsAttribute;

            // default settings
            _maxProcessingTrials = 5;

            if (null != settings) // settings are provided through custom attribute
            {
                _queueName = settings.QueueName ?? TypeMapper.GetStorageName(typeof(T));
                _serviceName = settings.ServiceName ?? GetType().FullName;

                if (settings.MaxProcessingTrials > 0)
                {
                    _maxProcessingTrials = settings.MaxProcessingTrials;
                }

            }
            else
            {
                _queueName = TypeMapper.GetStorageName(typeof(T));
                _serviceName = GetType().FullName;
            }

            // 1.25 * execution timeout, but limited to 2h max
            _visibilityTimeout = TimeSpan.FromSeconds(Math.Max(1, Math.Min(7200, (1.25 * ExecutionTimeout.TotalSeconds))));
        }

        /// <summary>Do not try to override this method, use <see cref="Start"/> instead.</summary>
        protected sealed override ServiceExecutionFeedback StartImpl()
        {
            var messages = Queues.Get<T>(_queueName, MaxAzureQueuePullSize, _visibilityTimeout, _maxProcessingTrials).ToList();

            var count = messages.Count();
            if (count > 0)
            {
                try
                {
                    StartRange(messages);
                }
                catch (ThreadAbortException)
                {
                    messages.ForEach(ResumeLater);
                    // no effect if the message has already been deleted, abandoned or resumed
                    throw;
                }
                catch (Exception)
                {
                    // no effect if the message has already been deleted, abandoned or resumed
                    messages.ForEach(Abandon);
                    throw;
                }
            }

            // Messages might have already been deleted by the 'Start' method.
            // It's OK, 'Delete' is idempotent.
            DeleteRange(messages);

            return count > 0
                ? ServiceExecutionFeedback.WorkAvailable
                : ServiceExecutionFeedback.Skipped;
        }

        private void ProcessMessage(T message)
        {
            try
            {
                Start(message);
            }
            catch (ThreadAbortException)
            {
                // no effect if the message has already been deleted, abandoned or resumed
                ResumeLater(message);
                throw;
            }
            catch (Exception)
            {
                // no effect if the message has already been deleted, abandoned or resumed
                Abandon(message);
                throw;
            }

            // no effect if the message has already been deleted, abandoned or resumed
            Delete(message);
        }

        /// <summary>Method called first by the <c>Lokad.Cloud</c> framework when a message is
        /// available for processing. The message is automatically deleted from the queue
        /// if the method returns (no deletion if an exception is thrown).</summary>
        protected virtual void Start(T message)
        {
            throw new NotSupportedException("Start or StartRange method must overridden by inheritor.");
        }

        /// <summary>Method called first by the <c>Lokad.Cloud</c> framework when messages are
        /// available for processing. Default implementation is naively calling <see cref="Start"/>.
        /// </summary>
        /// <param name="messages">Messages to be processed.</param>
        /// <remarks>
        /// We suggest to make messages deleted asap through the <see cref="DeleteRange"/>
        /// method. Otherwise, messages will be automatically deleted when the method
        /// returns (except if an exception is thrown obviously).
        /// </remarks>
        protected virtual void StartRange(IEnumerable<T> messages)
        {
            foreach (var message in messages)
            {
                ProcessMessage(message);
            }
        }

        /// <summary>
        /// Delete messages retrieved either through <see cref="StartRange"/>
        /// or through <see cref="GetMore(int)"/>.
        /// </summary>
        public void DeleteRange(IEnumerable<T> messages)
        {
            Queues.DeleteRange(messages);
        }

        /// <summary>
        /// Delete message retrieved through <see cref="Start"/>.
        /// </summary>
        public void Delete(T message)
        {
            Queues.Delete(message);
        }

        /// <summary>
        /// Abandon a messages retrieved through <see cref="Start"/>
        /// and put it visibly back on the queue.
        /// </summary>
        public void Abandon(T message)
        {
            Queues.Abandon(message);
        }

        /// <summary>
        /// Resume a message retrieved through <see cref="Start"/>
        /// later and put it visibly back on the queue,
        /// without decreasing the poison detection dequeue count.
        /// </summary>
        public void ResumeLater(T message)
        {
            Queues.ResumeLater(message);
        }
    }
}
