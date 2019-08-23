using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using TwainDotNet.TwainNative;

namespace TwainDotNet
{
    public class DataSource : IDisposable
    {
		/// <summary>
		/// The logger for this class.
		/// </summary>
		static ILog log = LogManager.GetLogger( typeof( DataSource ) );

		Identity _applicationId;
        IWindowsMessageHook _messageHook;
        private IEnumerable<Capabilities> supportedCapabilities;
        private IEnumerable<Capabilities> extendedCapabilities;

        public DataSource( Identity applicationId, Identity sourceId, IWindowsMessageHook messageHook )
        {
            _applicationId = applicationId;
            SourceId = sourceId.Clone();
            _messageHook = messageHook;
        }

        ~DataSource()
        {
            Dispose( false );
        }

        public Identity SourceId
        {
            get; private set;
        }

        public int State
        {
            get; set;
        } = 0;

        public void NegotiateTransferCount( ScanSettings scanSettings )
        {
            try
            {
                scanSettings.TransferCount = ( short )Capability.SetCapability(
                        Capabilities.XferCount,
                        scanSettings.TransferCount,
                        TwainType.Int16,
                        _applicationId,
                        SourceId );
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        public void NegotiateFeeder( ScanSettings scanSettings )
        {
            try
            {
                if( scanSettings.UseDocumentFeeder.HasValue )
                {
                    NegotiateCapability( Capabilities.FeederEnabled, scanSettings.UseDocumentFeeder.Value );
					if( scanSettings.UseDocumentFeeder.Value )
					{
						if( scanSettings.UseAutoFeeder.HasValue )
						{
							NegotiateCapability( Capabilities.AutoFeed, scanSettings.UseAutoFeeder == true && scanSettings.UseDocumentFeeder == true );
						}

						if( scanSettings.UseAutoScanCache.HasValue )
						{
							NegotiateCapability( Capabilities.AutoScan, scanSettings.UseAutoScanCache.Value );
						}
					}
				}
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        public PixelType GetPixelType( ScanSettings scanSettings )
        {
            switch( scanSettings.Resolution.ColourSetting )
            {
                case ColourSetting.BlackAndWhite:
                    return PixelType.BlackAndWhite;

                case ColourSetting.GreyScale:
                    return PixelType.Grey;

                case ColourSetting.Colour:
                    return PixelType.Rgb;
            }

            throw new NotImplementedException();
        }

        public short GetBitDepth( ScanSettings scanSettings )
        {
            switch( scanSettings.Resolution.ColourSetting )
            {
                case ColourSetting.BlackAndWhite:
                    return 1;

                case ColourSetting.GreyScale:
                    return 8;

                case ColourSetting.Colour:
                    return 24;
            }

            throw new NotImplementedException();
        }

        public bool PaperDetectable
        {
            get
            {
                try
                {
                    return Capability.GetBoolCapability( Capabilities.FeederLoaded, _applicationId, SourceId );
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool SupportsDuplex
        {
            get
            {
                if( !supportedCapabilities.Contains( Capabilities.Duplex ) )
                {
                    return false;
                }

                try
                {
                    var cap = new Capability( Capabilities.Duplex, _applicationId, SourceId );
                    return ( ( Duplex )cap.GetBasicValue().Int16Value ) != Duplex.None;
                }
                catch
                {
                    return false;
                }
            }
        }

        public void NegotiateColour( ScanSettings scanSettings )
        {
            if( scanSettings.Resolution.ColourSetting == ColourSetting.Default )
            {
                return;
            }

            if( supportedCapabilities.Contains( Capabilities.IPixelType ) )
            {
                try
                {
                    Capability.SetCapability( Capabilities.IPixelType, ( ushort )GetPixelType( scanSettings ), TwainType.UInt16, _applicationId, SourceId );
                }
                catch
                {
                    // Do nothing if the data source does not support the requested capability
                }
            }

            // TODO: Also set this for colour scanning
            if( supportedCapabilities.Contains( Capabilities.BitDepth ) )
            {
                try
                {
                    Capability.SetCapability( Capabilities.BitDepth, GetBitDepth( scanSettings ), TwainType.UInt16, _applicationId, SourceId );
                }
                catch
                {
                    // Do nothing if the data source does not support the requested capability
                }
            }
        }

        public void NegotiateResolution( ScanSettings scanSettings )
        {
            try
            {
                if( scanSettings.Resolution.Dpi.HasValue )
                {
                    // Resolution is of type TW_FIX32.
                    Fix32 dpi = new Fix32( scanSettings.Resolution.Dpi.Value );
                    NegotiateCapability( Capabilities.XResolution, dpi );
                    NegotiateCapability( Capabilities.YResolution, dpi );
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        private void NegotiateCapability( Capabilities capabilities, Fix32 fix32 )
        {
            if( !supportedCapabilities.Contains( capabilities ) )
            {
                return;
            }

            CapabilityResult capabilityResult = Capability.GetCapability( capabilities, _applicationId, SourceId );
            TwainType expectedTwainType = TwainType.Fix32;
            if( capabilityResult.TwainType != expectedTwainType )
            {
                throw new TwainException( string.Format( "Capability {0} TwainType mismatch. Expected: {1}; Actual: {2}", capabilities, expectedTwainType, capabilityResult.TwainType ) );
            }

            bool allowSet = false;
            if( capabilityResult is EnumCapabilityResult enumCapabilityResult && enumCapabilityResult.ItemCount > 1 )
            {
                allowSet = true;
            }
            if( capabilityResult is RangeCapabilityResult rangeCapabilityResult && rangeCapabilityResult.MinValue != rangeCapabilityResult.MaxValue )
            {
                allowSet = true;
            }
            if( capabilityResult is ArrayCapabilityResult arrayCapabilityResult )
            {
                throw new TwainException( "Unable to Set Array capability." );
            }

            if( allowSet )
            {
                Capability.SetCapability( capabilities, fix32, _applicationId, SourceId );
            }
        }

        private void NegotiateCapability( Capabilities capabilities, bool value )
        {
            if( !supportedCapabilities.Contains( capabilities ) )
            {
                return;
            }

            Capability.SetCapability( capabilities, value, _applicationId, SourceId );
        }

        public void NegotiateDuplex( ScanSettings scanSettings )
        {
            try
            {
                if( scanSettings.UseDuplex.HasValue && SupportsDuplex )
                {
                    if( !supportedCapabilities.Contains( Capabilities.DuplexEnabled ) )
                    {
                        return;
                    }

                    Capability.SetCapability( Capabilities.DuplexEnabled, scanSettings.UseDuplex.Value, _applicationId, SourceId );
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        public void NegotiateOrientation( ScanSettings scanSettings )
        {
            if( !supportedCapabilities.Contains( Capabilities.Orientation ) )
            {
                return;
            }

            // Set orientation (default is portrait)
            try
            {
                var cap = new Capability( Capabilities.Orientation, _applicationId, SourceId );
                if( ( Orientation )cap.GetBasicValue().Int16Value != Orientation.Default )
                {
                    Capability.SetCapability( Capabilities.Orientation, ( ushort )scanSettings.Page.Orientation, TwainType.UInt16, _applicationId, SourceId );
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        /// <summary>
        /// Negotiates the size of the page.
        /// </summary>
        /// <param name="scanSettings">The scan settings.</param>
        public void NegotiatePageSize( ScanSettings scanSettings )
        {
            if( !supportedCapabilities.Contains( Capabilities.Supportedsizes ) )
            {
                return;
            }

            try
            {
                var cap = new Capability( Capabilities.Supportedsizes, _applicationId, SourceId );
                if( ( PageType )cap.GetBasicValue().Int16Value != PageType.UsLetter )
                {
                    Capability.SetCapability( Capabilities.Supportedsizes, ( ushort )scanSettings.Page.Size, TwainType.UInt16, _applicationId, SourceId );
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        /// <summary>
        /// Negotiates the automatic rotation capability.
        /// </summary>
        /// <param name="scanSettings">The scan settings.</param>
        public void NegotiateAutomaticRotate( ScanSettings scanSettings )
        {
            try
            {
                if( scanSettings.Rotation.AutomaticRotate )
                {
                    NegotiateCapability( Capabilities.Automaticrotate, true );
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        /// <summary>
        /// Negotiates the automatic border detection capability.
        /// </summary>
        /// <param name="scanSettings">The scan settings.</param>
        public void NegotiateAutomaticBorderDetection( ScanSettings scanSettings )
        {
            try
            {
                if( scanSettings.Rotation.AutomaticBorderDetection )
                {
                    NegotiateCapability( Capabilities.Automaticborderdetection, true );
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        /// <summary>
        /// Negotiates the indicator.
        /// </summary>
        /// <param name="scanSettings">The scan settings.</param>
        public void NegotiateProgressIndicator( ScanSettings scanSettings )
        {
            try
            {
                if( scanSettings.ShowProgressIndicatorUI.HasValue )
                {
                    NegotiateCapability( Capabilities.Indicators, scanSettings.ShowProgressIndicatorUI.Value );
                }
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        /// <summary>
        /// Negotiates the indicator.
        /// </summary>
        /// <param name="scanSettings">The scan settings.</param>
        public void NegotiateDataTransferMode( ScanSettings scanSettings )
        {
            try
            {
                Capability.SetCapability( Capabilities.IXferMech, ( short )scanSettings.DataTransferMode, TwainType.UInt16, _applicationId, SourceId );
            }
            catch( Exception ex )
            {
                // Do nothing if the data source does not support the requested capability
            }
        }

        public bool Open( ScanSettings settings )
        {
            OpenSource();

            supportedCapabilities = GetSupportedCapabilities();
            extendedCapabilities = GetExtendedCapabilities();

            if( settings.AbortWhenNoPaperDetectable && !PaperDetectable )
                throw new FeederEmptyException();

			if( settings.DebugCapabilities )
			{
				this.DebugCapabilities();
			}

			// Set whether or not to show progress window
			NegotiateProgressIndicator( settings );
            NegotiateTransferCount( settings );
            NegotiateFeeder( settings );
            NegotiateDuplex( settings );
            NegotiateDataTransferMode( settings );

            if( settings.UseDocumentFeeder == true &&
                settings.Page != null )
            {
                NegotiatePageSize( settings );
                NegotiateOrientation( settings );
            }

			if( NegotiateUnits( settings ) )
			{
				if( settings.Area != null )
				{
					NegotiateArea( settings );
				}
			}

			if( settings.Resolution != null )
            {
                NegotiateColour( settings );
                NegotiateResolution( settings );
            }

            // Configure automatic rotation and image border detection
            if( settings.Rotation != null )
            {
                NegotiateAutomaticRotate( settings );
                NegotiateAutomaticBorderDetection( settings );
            }

            return Enable( settings );
        }

        private IEnumerable<Capabilities> GetSupportedCapabilities()
        {
            CapabilityResult capabilityResult = Capability.GetCapability( Capabilities.SupportedCapabilities, _applicationId, SourceId );
            if( capabilityResult is ArrayCapabilityResult arrayCapabilityResult )
            {
                return arrayCapabilityResult.GetUInt16Items().Cast<Capabilities>().ToArray();
            }
            return new Capabilities[] { };
        }

        private IEnumerable<Capabilities> GetExtendedCapabilities()
        {
            if( supportedCapabilities.Contains( Capabilities.Extendedcaps ) )
            {
                CapabilityResult capabilityResult = Capability.GetCapability( Capabilities.Extendedcaps, _applicationId, SourceId );
                if( capabilityResult is ArrayCapabilityResult arrayCapabilityResult )
                {
                    return arrayCapabilityResult.GetUInt16Items().Cast<Capabilities>().ToArray();
                }
            }
            return new Capabilities[] { };
        }

		private void DebugCapabilities()
		{
			log.Debug( "Start debugging capabilities." );
			foreach( Capabilities supportedCapability in supportedCapabilities )
			{
				if( supportedCapability == Capabilities.SupportedCapabilities )
				{
					continue;
				}

				try
				{
					CapabilityResult capabilityResult = Capability.GetCapability( supportedCapability, _applicationId, SourceId );
					log.DebugFormat( "{0} {1}", supportedCapability, capabilityResult.ToString() );
				}
				catch( Exception ex )
				{
					log.DebugFormat( "{0} {1}", supportedCapability, ex.Message );
				}
			}
			log.Debug( "End debugging capabilities." );
		}

		private bool NegotiateUnits( ScanSettings scanSettings )
        {
            Units requestedUnits = scanSettings.Units;

            if( !supportedCapabilities.Contains( Capabilities.IUnits ) )
            {
                return false;
            }

            try
            {
                CapabilityResult capabilityResult = Capability.GetCapability( Capabilities.IUnits, _applicationId, SourceId );
                if( capabilityResult is BasicCapabilityResult basicCapabilityResult )
                {
                    if( basicCapabilityResult.UInt16Value == ( ushort )requestedUnits )
                    {
                        return true;
                    }
                    return false;
                }

                if( capabilityResult is EnumCapabilityResult enumCapabilityResult )
                {
                    Units[] units = enumCapabilityResult.GetUInt16Items().Cast<Units>().ToArray();
                    if( !units.Contains( requestedUnits ) )
                    {
                        return false;
                    }

                    if( units[enumCapabilityResult.CurrentIndex] == requestedUnits )
                    {
                        return true;
                    }
                }

                Capability.SetCapability( Capabilities.IUnits, ( ushort )requestedUnits, TwainType.UInt16, _applicationId, SourceId );

                return true;
            }
            catch
            {
                // Do nothing if the data source does not support the requested capability
                return false;
            }
        }

        private bool NegotiateArea( ScanSettings scanSettings )
        {
            var area = scanSettings.Area;

            if( area == null )
            {
                return false;
            }

            var imageLayout = new ImageLayout
            {
                Frame = new Frame
                {
                    Left = new Fix32( area.Left ),
                    Top = new Fix32( area.Top ),
                    Right = new Fix32( area.Right ),
                    Bottom = new Fix32( area.Bottom )
                }
            };

            var result = Twain32Native.DsImageLayout(
                _applicationId,
                SourceId,
                DataGroup.Image,
                DataArgumentType.ImageLayout,
                Message.Set,
                imageLayout );

            switch( result )
            {
                case TwainResult.Success:
                    break;
                case TwainResult.CheckStatus:
                    result = Twain32Native.DsImageLayout(
                                    _applicationId,
                                    SourceId,
                                    DataGroup.Image,
                                    DataArgumentType.ImageLayout,
                                    Message.Get,
                                    imageLayout );

                    scanSettings.Area = new AreaSettings( imageLayout.Frame.Top.ToFloat(), imageLayout.Frame.Left.ToFloat(), imageLayout.Frame.Bottom.ToFloat(), imageLayout.Frame.Right.ToFloat() );
                    break;
                default:
                    ConditionCode conditionCode = DataSourceManager.GetConditionCode( _applicationId, SourceId );

                    throw new TwainException( string.Format( "DsImageLayout.Set {0}; {1}.", result, conditionCode ), result, conditionCode );
            }

            return true;
        }

        public void OpenSource()
        {
            var result = Twain32Native.DsmIdentity(
                   _applicationId,
                   IntPtr.Zero,
                   DataGroup.Control,
                   DataArgumentType.Identity,
                   Message.OpenDS,
                   SourceId );

            if( result != TwainResult.Success )
            {
                ConditionCode conditionCode = DataSourceManager.GetConditionCode( _applicationId, null );
                throw new TwainException( $"Error opening data source {result} {conditionCode}.", result, conditionCode );
            }

            this.State = 4;
        }

        public bool Enable( ScanSettings settings )
        {
            bool uiControllable = false;
            if( this.supportedCapabilities.Contains( Capabilities.UIControllable ) )
            {
                try
                {
                    uiControllable = Capability.GetBoolCapability( Capabilities.UIControllable, _applicationId, SourceId );
                }
                catch( Exception )
                {
                }
            }

            UserInterface ui = new UserInterface();
            ui.ShowUI = ( short )( !uiControllable || settings.ShowTwainUI ? 1 : 0 );
            ui.ModalUI = 1;
            ui.ParentHand = _messageHook.WindowHandle;

            var result = Twain32Native.DsUserInterface(
                _applicationId,
                SourceId,
                DataGroup.Control,
                DataArgumentType.UserInterface,
                Message.EnableDS,
                ui );

            switch( result )
            {
                case TwainResult.Success:
                    State = 5;
                    break;

                case TwainResult.CheckStatus:
                    // Sources user interface is being displayed; despite settings.ShowTwainUI = false.
                    State = 5;
                    break;

                case TwainResult.Cancel:
					// Not documented; but can happen when user cancels UI dialog.
					log.Warn( "Enable DS cancelled." );
                    return false;
                default:
                    ConditionCode conditionCode = DataSourceManager.GetConditionCode( _applicationId, SourceId );
                    throw new TwainException( $"Error opening data source {result} {conditionCode}.", result, conditionCode );
            }
            return true;
        }

        public static DataSource GetDefault( Identity applicationId, IWindowsMessageHook messageHook )
        {
            var defaultSourceId = new Identity();

            // Attempt to get information about the system default source
            var result = Twain32Native.DsmIdentity(
                applicationId,
                IntPtr.Zero,
                DataGroup.Control,
                DataArgumentType.Identity,
                Message.GetDefault,
                defaultSourceId );

            if( result != TwainResult.Success )
            {
                var status = DataSourceManager.GetConditionCode( applicationId, null );
                throw new TwainException( "Error getting information about the default source: " + result, result, status );
            }

            return new DataSource( applicationId, defaultSourceId, messageHook );
        }

        public static DataSource UserSelected( Identity applicationId, IWindowsMessageHook messageHook )
        {
            var defaultSourceId = new Identity();

            // Show the TWAIN interface to allow the user to select a source
            Twain32Native.DsmIdentity(
                applicationId,
                IntPtr.Zero,
                DataGroup.Control,
                DataArgumentType.Identity,
                Message.UserSelect,
                defaultSourceId );

            return new DataSource( applicationId, defaultSourceId, messageHook );
        }

        public static List<DataSource> GetAllSources( Identity applicationId, IWindowsMessageHook messageHook )
        {
            var sources = new List<DataSource>();
            Identity id = new Identity();

            // Get the first source
            var result = Twain32Native.DsmIdentity(
                applicationId,
                IntPtr.Zero,
                DataGroup.Control,
                DataArgumentType.Identity,
                Message.GetFirst,
                id );

            if( result == TwainResult.EndOfList )
            {
                return sources;
            }
            else if( result != TwainResult.Success )
            {
                throw new TwainException( "Error getting first source.", result );
            }
            else
            {
                sources.Add( new DataSource( applicationId, id, messageHook ) );
            }

            while( true )
            {
                // Get the next source
                result = Twain32Native.DsmIdentity(
                    applicationId,
                    IntPtr.Zero,
                    DataGroup.Control,
                    DataArgumentType.Identity,
                    Message.GetNext,
                    id );

                if( result == TwainResult.EndOfList )
                {
                    break;
                }
                else if( result != TwainResult.Success )
                {
                    throw new TwainException( "Error enumerating sources.", result );
                }

                sources.Add( new DataSource( applicationId, id, messageHook ) );
            }

            return sources;
        }

        public static DataSource GetSource( string sourceProductName, Identity applicationId, IWindowsMessageHook messageHook )
        {
            // A little slower than it could be, if enumerating unnecessary sources is slow. But less code duplication.
            foreach( var source in GetAllSources( applicationId, messageHook ) )
            {
                if( sourceProductName.Equals( source.SourceId.ProductName, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    return source;
                }
            }

            return null;
        }


        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected void Dispose( bool disposing )
        {
            if( disposing )
            {
                Close();
            }
        }

        public void Close()
        {
            if( SourceId.Id != 0 )
            {
                ConditionCode conditionCode;
                TwainResult result;
                if( this.State >= 5 )
                {
                    UserInterface userInterface = new UserInterface();



                    result = Twain32Native.DsUserInterface(
                        _applicationId,
                        SourceId,
                        DataGroup.Control,
                        DataArgumentType.UserInterface,
                        Message.DisableDS,
                        userInterface );

                    if( result == TwainResult.Success )
                    {
                        this.State = 4;
                    }
                    else
                    {
                        conditionCode = DataSourceManager.GetConditionCode( _applicationId, SourceId );
                    }
                }

                if( this.State >= 4 )
                {
                    result = Twain32Native.DsmIdentity(
                        _applicationId,
                        IntPtr.Zero,
                        DataGroup.Control,
                        DataArgumentType.Identity,
                        Message.CloseDS,
                        SourceId );

                    if( result == TwainResult.Success )
                    {
                        State = 3;
                    }
                    else
                    {
                        conditionCode = DataSourceManager.GetConditionCode( _applicationId, SourceId );
                    }
                }
            }
        }
    }
}
