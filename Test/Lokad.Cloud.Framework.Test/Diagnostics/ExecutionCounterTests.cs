﻿#region Copyright (c) Lokad 2009-2011
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System;
using System.Linq;
using Autofac;
using Lokad.Cloud.Diagnostics;
using Lokad.Cloud.Management;
using Lokad.Cloud.Test;
using NUnit.Framework;
using Lokad.Diagnostics;
using Lokad.Cloud.Storage.Shared.Diagnostics;

namespace Lokad.Cloud.Diagnostics.Test
{
    [TestFixture]
    public class ExecutionCounterTests
    {
        [Test]
        public void ExecutionProfilesMakeItToTheStatistics()
        {
            var monitor = new ExecutionProfilingMonitor(GlobalSetup.Container.Resolve<ICloudDiagnosticsRepository>());
            var statistics = new CloudStatistics(GlobalSetup.Container.Resolve<ICloudDiagnosticsRepository>());

            var count = statistics.GetProfilesOfDay(DateTime.UtcNow)
                .SelectMany(s => s.Statistics)
                .Where(e => e.Name.Contains("Test Profile"))
                .Sum(e => e.OpenCount);

            var counter = new ExecutionCounter("Test Profile", 0, 0);
            var timestamp = counter.Open();
            counter.Close(timestamp);

            ExecutionCounters.Default.RegisterRange(new[] {counter});
            monitor.UpdateDefaultStatistics();

            Assert.AreEqual(
                count + 1,
                statistics.GetProfilesOfDay(DateTime.UtcNow)
                    .SelectMany(s => s.Statistics)
                    .Where(e => e.Name.Contains("Test Profile"))
                    .Sum(e => e.OpenCount));
        }
    }
}