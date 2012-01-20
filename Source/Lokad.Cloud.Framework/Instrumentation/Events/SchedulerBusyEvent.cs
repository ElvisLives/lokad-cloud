﻿#region Copyright (c) Lokad 2011
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;

namespace Lokad.Cloud.Instrumentation.Events
{
    /// <summary>
    /// Raised whenever the runtime scheduler becomes busy.
    /// </summary>
    public class SchedulerBusyEvent : ICloudRuntimeEvent
    {
        public DateTimeOffset Timestamp { get; private set; }

        public SchedulerBusyEvent(DateTimeOffset timestamp)
        {
            Timestamp = timestamp;
        }
    }
}
