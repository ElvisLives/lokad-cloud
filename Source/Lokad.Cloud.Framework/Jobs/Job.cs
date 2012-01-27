﻿using System;
using System.Runtime.Serialization;

namespace Lokad.Cloud.Jobs
{
    [DataContract(Namespace = "http://schemas.lokad.com/lokad-cloud/jobs/1.1"), Serializable]
    public class Job
    {
        [DataMember(Order = 1, IsRequired = true)]
        public string JobId { get; set; }
    }
}
