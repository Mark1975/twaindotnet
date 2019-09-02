using System;
using System.Windows.Forms;

namespace TwainDotNet.WinFroms
{
    /// <summary>
    /// A windows message hook for WinForms applications.
    /// </summary>
    public class WinFormsWindowMessageHook : IWindowsMessageHook, IMessageFilter
    {
        readonly IntPtr _windowHandle;
        bool _usingFilter;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="window">The window.</param>
        public WinFormsWindowMessageHook(Form window)
        {
            _windowHandle = window.Handle;
        }

		/// <summary>
		/// Pre filter message.
		/// </summary>
		/// <param name="m">The message.</param>
		/// <returns></returns>
        public bool PreFilterMessage(ref Message m)
        {
            if (FilterMessageCallback != null)
            {
                bool handled = false;
                FilterMessageCallback(m.HWnd, m.Msg, m.WParam, m.LParam, ref handled);
                return handled;
            }

            return false;
        }

		/// <summary>
		/// Gets the window handle.
		/// </summary>
        public IntPtr WindowHandle { get { return _windowHandle; } }

		/// <summary>
		/// Gets or sets whether to use the filter.
		/// </summary>
        public bool UseFilter
        {
            get
            {
                return _usingFilter;
            }
            set
            {
                if (!_usingFilter && value == true)
                {
                    Application.AddMessageFilter(this);
                    _usingFilter = true;
                }

                if (_usingFilter && value == false)
                {
                    Application.RemoveMessageFilter(this);
                    _usingFilter = false;
                }
            }
        }

		/// <summary>
		/// Gets or sets the filter message callback.
		/// </summary>
        public FilterMessage FilterMessageCallback { get; set; }        
    }
}
