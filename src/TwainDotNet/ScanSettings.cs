using System.ComponentModel;
using TwainDotNet.TwainNative;

namespace TwainDotNet
{
	/// <summary>
	/// Scan settings.
	/// </summary>
	public class ScanSettings : INotifyPropertyChanged
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public ScanSettings()
		{
		}

		bool _showTwainUI;

		/// <summary>
		/// Indicates if the TWAIN/driver user interface should be used to pick the scan settings.
		/// </summary>
		public bool ShowTwainUI
		{
			get
			{
				return _showTwainUI;
			}
			set
			{
				if( value != _showTwainUI )
				{
					_showTwainUI = value;
					OnPropertyChanged( "ShowTwainUI" );
				}
			}
		}

		bool? _showProgressIndicatorUI;

		/// <summary>
		/// Gets or sets a value indicating whether [show progress indicator ui].
		/// If TRUE, the Source will display a progress indicator during acquisition and transfer, regardless of whether the Source's user interface is active. 
		/// If FALSE, the progress indicator will be suppressed if the Source's user interface is inactive.
		/// The Source will continue to display device-specific instructions and error messages even with the Source user interface and progress indicators turned off. 
		/// </summary>
		/// <value><c>true</c> if [show progress indicator ui]; otherwise, <c>false</c>.</value>
		public bool? ShowProgressIndicatorUI
		{
			get
			{
				return _showProgressIndicatorUI;
			}
			set
			{
				if( value != _showProgressIndicatorUI )
				{
					_showProgressIndicatorUI = value;
					OnPropertyChanged( "ShowProgressIndicatorUI" );
				}
			}
		}

		bool? _useDocumentFeeder;

		/// <summary>
		/// Indicates if the automatic document feeder (ADF) should be the source of the document(s) to scan.
		/// </summary>
		public bool? UseDocumentFeeder
		{
			get
			{
				return _useDocumentFeeder;
			}
			set
			{
				if( value != _useDocumentFeeder )
				{
					_useDocumentFeeder = value;
					OnPropertyChanged( "UseDocumentFeeder" );
				}
			}
		}

		bool? _useAutoFeeder;

		/// <summary>
		/// Indicates if the automatic document feeder (ADF) should continue feeding document(s) to scan after the negotiated number of pages are acquired.
		/// UseDocumentFeeder must be true
		/// </summary>
		public bool? UseAutoFeeder
		{
			get
			{
				return _useAutoFeeder;
			}
			set
			{
				if( value != _useAutoFeeder )
				{
					_useAutoFeeder = value;
					OnPropertyChanged( "UseAutoFeeder" );
				}
			}
		}

		bool? _useAutoScanCache;

		/// <summary>
		/// Indicates if the source should continue scanning without waiting for the application to request the image transfers.
		/// </summary>
		public bool? UseAutoScanCache
		{
			get
			{
				return _useAutoScanCache;
			}
			set
			{
				if( value != _useAutoScanCache )
				{
					_useAutoScanCache = value;
					OnPropertyChanged( "UseAutoScanCache" );
				}
			}
		}

		bool _abortWhenNoPaperDetectable;

		/// <summary>
		/// Indicates if the transfer should not start when no paper was detected (e.g. by the ADF).
		/// </summary>
		public bool AbortWhenNoPaperDetectable
		{
			get
			{
				return _abortWhenNoPaperDetectable;
			}
			set
			{
				if( value != _abortWhenNoPaperDetectable )
				{
					_abortWhenNoPaperDetectable = value;
					OnPropertyChanged( "AbortWhenNoPaperDetectable" );
				}
			}
		}

		short? _transferCount;

		/// <summary>
		/// The number of pages to transfer.
		/// </summary>
		public short? TransferCount
		{
			get
			{
				return _transferCount;
			}
			set
			{
				if( value != _transferCount )
				{
					_transferCount = value;
					OnPropertyChanged( "TransferCount" );
					OnPropertyChanged( "ShouldTransferAllPages" );
				}
			}
		}

		/// <summary>
		/// Indicates if all pages should be transferred.
		/// </summary>
		public bool ShouldTransferAllPages
		{
			get
			{
				return _transferCount == TransferAllPages;
			}
			set
			{
				TransferCount = value ? TransferAllPages : ( short )1;
			}
		}

		bool? _useDuplex;

		/// <summary>
		/// Whether to use duplexing, if supported.
		/// </summary>
		public bool? UseDuplex
		{
			get
			{
				return _useDuplex;
			}
			set
			{
				if( value != _useDuplex )
				{
					_useDuplex = value;
					OnPropertyChanged( "UseDuplex" );
				}
			}
		}

		AreaSettings _area;
		/// <summary>
		/// Gets or sets the area.
		/// </summary>

		public AreaSettings Area
		{
			get
			{
				return _area;
			}
			set
			{
				if( value != _area )
				{
					_area = value;
					OnPropertyChanged( "Area" );
				}
			}
		}

		TwainNative.TransferMechanism? _dataTransferMode;

		/// <summary>
		/// Gets or sets the data transfer mode.
		/// </summary>
		public TwainNative.TransferMechanism? DataTransferMode
		{
			get
			{
				return _dataTransferMode;
			}
			set
			{
				if( value != _dataTransferMode )
				{
					_dataTransferMode = value;
					OnPropertyChanged( "DataTransferMode" );
				}
			}
		}

		private Units? _units;
		/// <summary>
		/// Gets or sets the units.
		/// </summary>
		public Units? Units
		{
			get
			{
				return _units;
			}
			set
			{
				_units = value;
				OnPropertyChanged( "Units" );
			}
		}

		/// <summary>
		/// Gets or sets the units options.
		/// </summary>
		public Units[] UnitsOptions
		{
			get;
			set;
		}

		float? _dpi;

		/// <summary>
		/// The DPI to scan at. Set to null to use the current default setting.
		/// </summary>
		public float? Dpi
		{
			get
			{
				return _dpi;
			}
			set
			{
				if( value != _dpi )
				{
					_dpi = value;
					OnPropertyChanged( "Dpi" );
				}
			}
		}

		ColourSetting _colourSettings;

		/// <summary>
		/// The colour settings to use.
		/// </summary>
		public ColourSetting ColourSetting
		{
			get
			{
				return _colourSettings;
			}
			set
			{
				if( value != _colourSettings )
				{
					_colourSettings = value;
					OnPropertyChanged( "ColourSetting" );
				}
			}
		}

		private bool? automaticBorderDetection;
		/// <summary>
		/// Gets or sets a value indicating whether [automatic border detection].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [automatic border detection]; otherwise, <c>false</c>.
		/// </value>
		public bool? AutomaticBorderDetection
		{
			get
			{
				return automaticBorderDetection;
			}
			set
			{
				if( value != automaticBorderDetection )
				{
					automaticBorderDetection = value;
					OnPropertyChanged( "AutomaticBorderDetection" );
				}
			}
		}

		private bool? automaticDeskew;
		private bool? automaticRotate;
		private FlipRotation? flipRotation;

		/// <summary>
		/// Gets or sets a value indicating whether [automatic deskew].
		/// </summary>
		/// <value><c>true</c> if [automatic deskew]; otherwise, <c>false</c>.</value>
		public bool? AutomaticDeskew
		{
			get
			{
				return automaticDeskew;
			}
			set
			{
				if( value != automaticDeskew )
				{
					automaticDeskew = value;
					OnPropertyChanged( "AutomaticDeskew" );
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether [automatic rotate].
		/// </summary>
		/// <value><c>true</c> if [automatic rotate]; otherwise, <c>false</c>.</value>
		public bool? AutomaticRotate
		{
			get
			{
				return automaticRotate;
			}
			set
			{
				if( value != automaticRotate )
				{
					automaticRotate = value;
					OnPropertyChanged( "AutomaticRotate" );
				}
			}
		}

		/// <summary>
		/// Gets or sets the flip side rotation.
		/// </summary>
		/// <value>The flip side rotation.</value>
		public FlipRotation? FlipRotation
		{
			get
			{
				return flipRotation;
			}
			set
			{
				if( value != flipRotation )
				{
					flipRotation = value;
					OnPropertyChanged( "FlipRotation" );
				}
			}
		}

		Orientation? _orientation;

		/// <summary>
		/// Gets or sets the page orientation.
		/// </summary>
		/// <value>The orientation.</value>
		public Orientation? Orientation
		{
			get
			{
				return _orientation;
			}
			set
			{
				if( value != _orientation )
				{
					_orientation = value;
					OnPropertyChanged( "Orientation" );
				}
			}
		}

		PageType? _size;
		/// <summary>
		/// Gets or sets the Page Size.
		/// </summary>
		/// <value>The size.</value>
		public PageType? Size
		{
			get
			{
				return _size;
			}
			set
			{
				if( value != _size )
				{
					_size = value;
					OnPropertyChanged( "PaperSize" );
				}
			}
		}

		private bool _keepOpen;
		/// <summary>
		/// Gets or sets the Page Size.
		/// </summary>
		/// <value>The size.</value>
		public bool KeepOpen
		{
			get
			{
				return _keepOpen;
			}
			set
			{
				if( value != _keepOpen )
				{
					_keepOpen = value;
					OnPropertyChanged( "KeepOpen" );
				}
			}
		}

		bool _debugCapabilities;

		/// <summary>
		/// Indicates if the capabilities should be debugged when opening the DataSource.
		/// </summary>
		public bool DebugCapabilities
		{
			get
			{
				return _debugCapabilities;
			}
			set
			{
				if( value != _debugCapabilities )
				{
					_debugCapabilities = value;
					OnPropertyChanged( nameof( DebugCapabilities ) );
				}
			}
		}

		bool _prefer16bppGraycale;
		/// <summary>
		/// Indicates if the receiving program prefers 16bpp images when retrieving grayscale images.
		/// </summary>
		public bool Prefer16bppGraycale
		{
			get
			{
				return _prefer16bppGraycale;
			}
			set
			{
				if( value != _prefer16bppGraycale )
				{
					_prefer16bppGraycale = value;
					OnPropertyChanged( nameof( Prefer16bppGraycale ) );
				}
			}
		}

		#region INotifyPropertyChanged Members

		/// <summary>
		/// On property changed.
		/// </summary>
		/// <param name="propertyName">The property name.</param>
		protected void OnPropertyChanged( string propertyName )
		{
			PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
		}

		/// <summary>
		/// Property changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		#endregion

		/// <summary>
		/// Default scan settings.
		/// </summary>
		public static readonly ScanSettings Default = new ScanSettings()
		{
			ColourSetting = ColourSetting.Colour,
			Dpi = 300,
		};

		/// <summary>
		/// The value to set to scan all available pages.
		/// </summary>
		public const short TransferAllPages = -1;
	}

	/// <summary>
	/// Colour setting.
	/// </summary>
	public enum ColourSetting
	{
		/// <summary>
		/// Default.
		/// </summary>
		Default,

		/// <summary>
		/// Black and white.
		/// </summary>
		BlackAndWhite,

		/// <summary>
		/// Grey scale.
		/// </summary>
		GreyScale,

		/// <summary>
		/// Colour.
		/// </summary>
		Colour
	}
}
