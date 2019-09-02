using System;

namespace TwainDotNet
{
	/// <summary>
	/// Interface for windows message hook.
	/// </summary>
    public interface IWindowsMessageHook
    {
        /// <summary>
        /// Gets or sets if the message filter is in use.
        /// </summary>
        bool UseFilter { get; set; }

        /// <summary>
        /// The delegate to call back then the filter is in place and a message arrives.
        /// </summary>
        FilterMessage FilterMessageCallback { get; set; }

        /// <summary>
        /// The handle to the window that is performing the scanning.
        /// </summary>
        IntPtr WindowHandle { get; }
    }

	/// <summary>
	/// Filter message delegate.
	/// </summary>
	/// <param name="hwnd">The hwnd.</param>
	/// <param name="msg">The msg.</param>
	/// <param name="wParam">The wParam.</param>
	/// <param name="lParam">The lParam.</param>
	/// <param name="handled">Whether the message is handled.</param>
	/// <returns></returns>
    public delegate IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);
}
