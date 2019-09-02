using System;
using System.Collections.Generic;
using System.Text;

namespace TwainDotNet.TwainNative
{
    /// <summary>
    /// ICAP_XFERMECH values (Image Transfer)
    /// </summary>
    public enum TransferMechanism : ushort
    {
		/// <summary>
		/// Naive.
		/// </summary>
        Native = 0,

		/// <summary>
		/// File.
		/// </summary>
        File = 1,

		/// <summary>
		/// Memory.
		/// </summary>
        Memory = 2,

        // Value 3 was removed

		/// <summary>
		/// Mem file.
		/// </summary>
        MemFile = 4
    }
}
