using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace TwainDotNet.Wpf
{
	public class DebugWindowsMesage
	{
		public IntPtr hwnd
		{
			get; set;
		}
		public int msg
		{
			get; set;
		}
		public IntPtr wParam
		{
			get; set;
		}
		public IntPtr lParam
		{
			get; set;
		}
	}

	/// <summary>
	/// A windows message hook for WPF applications.
	/// </summary>
	public class WpfWindowMessageHook : IWindowsMessageHook
	{
		readonly HwndSource _source;
		readonly WindowInteropHelper _interopHelper;
		bool _usingFilter;
		public static List<DebugWindowsMesage> DebugWindowsMesages
		{
			get;
		} = new List<DebugWindowsMesage>();

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
				DebugWindowsMesage debugWindowsMesage = new DebugWindowsMesage { hwnd = hwnd, msg = msg, wParam = wParam, lParam = lParam };
				lock( DebugWindowsMesages )
				{
					DebugWindowsMesages.Add( debugWindowsMesage );
				}
				try
				{
					return FilterMessageCallback( hwnd, msg, wParam, lParam, ref handled );
				}
				finally
				{
					lock( DebugWindowsMesages )
					{
						DebugWindowsMesages.Remove( debugWindowsMesage );
					}
				}
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
