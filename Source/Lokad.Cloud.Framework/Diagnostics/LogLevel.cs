﻿#region Copyright (c) Lokad 2009-2012
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

namespace Lokad.Cloud.Diagnostics
{
    /// <remarks></remarks>
    public enum LogLevel
    {
        /// <summary> Message is intended for debugging </summary>
        Debug,
        /// <summary> Informatory message </summary>
        Info,
        /// <summary> The message is about potential problem in the system </summary>
        Warn,
        /// <summary> Some error has occured </summary>
        Error,
        /// <summary> Message is associated with the critical problem </summary>
        Fatal,

        /// <summary>
        /// Highest possible level
        /// </summary>
        Max = int.MaxValue,
        /// <summary> Smallest logging level</summary>
        Min = int.MinValue
    }
}
