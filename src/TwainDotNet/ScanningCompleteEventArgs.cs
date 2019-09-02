using System;

namespace TwainDotNet
{
	/// <summary>
	/// Scanning complete event args.
	/// </summary>
    public class ScanningCompleteEventArgs : EventArgs
    {
		/// <summary>
		/// Gets the exception.
		/// </summary>
        public Exception Exception { get; private set; }

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="exception">The exception.</param>
        public ScanningCompleteEventArgs(Exception exception)
        {
            this.Exception = exception;
        }
    }
}
