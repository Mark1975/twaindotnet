using System;
using System.Windows;
using System.Windows.Interop;

namespace TwainDotNet.Wpf
{
	/// <summary>
	/// A windows message hook for WPF applications.
	/// </summary>
	public class WpfWindowMessageHook : IWindowsMessageHook
	{
		readonly HwndSource _source;
		readonly WindowInteropHelper _interopHelper;
		bool _usingFilter;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="window">The window.</param>
		public WpfWindowMessageHook( Window window )
		{
			_source = ( HwndSource )PresentationSource.FromDependencyObject( window );
			_interopHelper = new WindowInteropHelper( window );
		}

		/// <summary>
		/// Filter message.
		/// </summary>
		/// <param name="hwnd">The hwnd.</param>
		/// <param name="msg">The msg.</param>
		/// <param name="wParam">The wParam.</param>
		/// <param name="lParam">The lParam.</param>
		/// <param name="handled">Whether the message is handled.</param>
		/// <returns></returns>
		public IntPtr FilterMessage( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
		{
			if( FilterMessageCallback != null )
			{
				return FilterMessageCallback( hwnd, msg, wParam, lParam, ref handled );
			}

			return IntPtr.Zero;
		}

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
				if( !_usingFilter && value == true )
				{
					_source.AddHook( FilterMessage );
					_usingFilter = true;
				}

				if( _usingFilter && value == false )
				{
					_source.RemoveHook( FilterMessage );
					_usingFilter = false;
				}
			}
		}

		/// <summary>
		/// Get or sets the filter message callback.
		/// </summary>
		public FilterMessage FilterMessageCallback
		{
			get; set;
		}

		/// <summary>
		/// Gets the window handle.
		/// </summary>
		public IntPtr WindowHandle
		{
			get
			{
				return _interopHelper.Handle;
			}
		}
	}
}
