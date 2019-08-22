using System;
using TwainDotNet.TwainNative;
using System.Runtime.InteropServices;
using TwainDotNet.Win32;
using System.Reflection;
using log4net;

namespace TwainDotNet
{
    public class DataSourceManager : IDisposable
    {
        /// <summary>
        /// The logger for this class.
        /// </summary>
        static ILog log = LogManager.GetLogger( typeof( DataSourceManager ) );

        IWindowsMessageHook _messageHook;
        Event _eventMessage;

        public Identity ApplicationId
        {
            get; private set;
        }
        public DataSource DataSource
        {
            get; private set;
        }

        public DataSourceManager( Identity applicationId, IWindowsMessageHook messageHook )
        {
            // Make a copy of the identity in case it gets modified
            ApplicationId = applicationId.Clone();

            ScanningComplete += delegate
            {
            };
            TransferImage += delegate
            {
            };

            _messageHook = messageHook;
            _messageHook.FilterMessageCallback = FilterMessage;
            IntPtr windowHandle = _messageHook.WindowHandle;

            _eventMessage.EventPtr = Marshal.AllocHGlobal( Marshal.SizeOf( typeof( WindowsMessage ) ) );

            // Initialise the data source manager
            TwainResult result = Twain32Native.DsmParent(
                ApplicationId,
                IntPtr.Zero,
                DataGroup.Control,
                DataArgumentType.Parent,
                Message.OpenDSM,
                ref windowHandle );

            if( result == TwainResult.Success )
            {
                //according to the 2.0 spec (2-10) if (applicationId.SupportedGroups
                // | DataGroup.Dsm2) > 0 then we should call DM_Entry(id, 0, DG_Control, DAT_Entrypoint, MSG_Get, wh)
                //right here
                DataSource = DataSource.GetDefault( ApplicationId, _messageHook );
            }
            else
            {
                throw new TwainException( "Error initialising DSM: " + result, result );
            }
        }

        ~DataSourceManager()
        {
            Dispose( false );
        }

        /// <summary>
        /// Notification that the scanning has completed.
        /// </summary>
        public event EventHandler<ScanningCompleteEventArgs> ScanningComplete;

        public event EventHandler<TransferImageEventArgs> TransferImage;

        public IWindowsMessageHook MessageHook
        {
            get
            {
                return _messageHook;
            }
        }

        public void StartScan( ScanSettings settings )
        {
            bool scanning = false;

            try
            {
                _messageHook.UseFilter = true;
                scanning = DataSource.Open( settings );
            }
            finally
            {
                // Remove the message hook if scan setup failed
                if( !scanning )
                {
                    DataSource.Close();
                    EndingScan();
                }
            }
        }

        protected IntPtr FilterMessage( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
        {
            if( DataSource.SourceId.Id == 0 )
            {
                handled = false;
                return IntPtr.Zero;
            }

            int pos = User32Native.GetMessagePos();

            WindowsMessage message = new WindowsMessage();
            message.hwnd = hwnd;
            message.message = msg;
            message.wParam = wParam;
            message.lParam = lParam;
            message.time = User32Native.GetMessageTime();
            message.x = ( short )pos;
            message.y = ( short )( pos >> 16 );

            Marshal.StructureToPtr( message, _eventMessage.EventPtr, false );
            _eventMessage.Message = 0;

            TwainResult result = Twain32Native.DsEvent(
                ApplicationId,
                DataSource.SourceId,
                DataGroup.Control,
                DataArgumentType.Event,
                Message.ProcessEvent,
                ref _eventMessage );

            if( result == TwainResult.NotDSEvent )
            {
                handled = false;
                return IntPtr.Zero;
            }

            switch( _eventMessage.Message )
            {
                case Message.XFerReady:
                    Exception exception = null;
                    try
                    {
                        TransferPictures();
                    }
                    catch( Exception e )
                    {
                        exception = e;
                    }
                    CloseDsAndCompleteScanning( exception );
                    break;

                case Message.CloseDS:
                case Message.CloseDSOK:
                case Message.CloseDSReq:
                    CloseDsAndCompleteScanning( null );
                    break;

                case Message.DeviceEvent:
                    break;

                default:
                    break;
            }

            handled = true;
            return IntPtr.Zero;
        }

        protected void TransferPictures()
        {
            if( DataSource.SourceId.Id == 0 )
            {
                return;
            }

            DataSource.State = 6;
            Capability c = new Capability( Capabilities.IXferMech, ApplicationId, DataSource.SourceId );
            BasicCapabilityResult basicCapabilityResult = c.GetBasicValue();

            TransferMechanism transferMechanism = ( TransferMechanism )basicCapabilityResult.UInt16Value;
            // Make sure only supported transfer mechanism is selected.
            if( transferMechanism != TransferMechanism.Memory )
            {
                transferMechanism = TransferMechanism.Native;
            }

            PendingXfers pendingTransfer = new PendingXfers();
            TwainResult result;
            try
            {
                do
                {
                    pendingTransfer.Count = 0;
                    IntPtr hbitmap = IntPtr.Zero;
                    MemoryTransferData memoryTransferData = null;
                    try
                    {
                        // Get the image info
                        ImageInfo imageInfo = new ImageInfo();
                        result = Twain32Native.DsImageInfo(
                            ApplicationId,
                            DataSource.SourceId,
                            DataGroup.Image,
                            DataArgumentType.ImageInfo,
                            Message.Get,
                            imageInfo );

                        if( result != TwainResult.Success )
                        {
                            return;
                        }

                        switch( transferMechanism )
                        {
                            case TransferMechanism.Native:
                                // Transfer the image from the device
                                result = Twain32Native.DsImageTransfer(
                                    ApplicationId,
                                    DataSource.SourceId,
                                    DataGroup.Image,
                                    DataArgumentType.ImageNativeXfer,
                                    Message.Get,
                                    ref hbitmap );

                                switch( result )
                                {
                                    case TwainResult.XferDone:
                                        DataSource.State = 7;
                                        break;
                                    case TwainResult.Cancel:
                                        DataSource.State = 7;
                                        return;
                                    default:
                                        // Source remains in state 6.
                                        ConditionCode conditionCode = DataSourceManager.GetConditionCode( ApplicationId, DataSource.SourceId );
                                        throw new TwainException( $"ImageNativeXfer failed. {result}, {conditionCode}.", result, conditionCode );
                                }
                                break;

                            case TransferMechanism.Memory:
                                memoryTransferData = PerformMemoryTransfer( imageInfo, ref result );
                                if( result != TwainResult.XferDone )
                                {
                                    return;
                                }
                                break;

                            default:
                                throw new NotSupportedException( $"Transfermode {transferMechanism} is not supported." );
                        }
                    }
                    finally
                    {
                        // End pending transfers
                        if( DataSource.State == 7 )
                        {
                            result = Twain32Native.DsPendingTransfer(
                                ApplicationId,
                                DataSource.SourceId,
                                DataGroup.Control,
                                DataArgumentType.PendingXfers,
                                Message.EndXfer,
                                pendingTransfer );

                            if( result == TwainResult.Success )
                            {
                                DataSource.State = 6;
                                if( pendingTransfer.Count == 0 )
                                {
                                    DataSource.State = 5;
                                }
                            }
                        }
                    }


                    switch( transferMechanism )
                    {
                        case TransferMechanism.Native:
                            if( hbitmap == IntPtr.Zero )
                            {
                                log.Warn( "Transfer complete but bitmap pointer is still null." );
                            }
                            else
                            {
                                using( var renderer = new BitmapRenderer( hbitmap ) )
                                {
                                    TransferImageEventArgs args = new TransferImageEventArgs( renderer.RenderToBitmap(), pendingTransfer.Count != 0 );
                                    TransferImage( this, args );
                                    if( !args.ContinueScanning )
                                        return;
                                }
                            }
                            break;
                        case TransferMechanism.Memory:
                            if( memoryTransferData == null )
                            {
                                log.Warn( "Transfer complete but memory transfer data is still null." );
                            }
                            else
                            {
                                TransferImageEventArgs args = new TransferImageEventArgs( memoryTransferData, pendingTransfer.Count != 0 );
                                TransferImage( this, args );
                                if( !args.ContinueScanning )
                                    return;
                            }
                            break;
                    }
                }
                while( DataSource.State == 6 );
            }
            finally
            {
                if( DataSource.State == 6 )
                {
                    // Reset any pending transfers
                    result = Twain32Native.DsPendingTransfer(
                        ApplicationId,
                        DataSource.SourceId,
                        DataGroup.Control,
                        DataArgumentType.PendingXfers,
                        Message.Reset,
                        pendingTransfer );

                    if( result == TwainResult.Success )
                    {
                        DataSource.State = 5;
                    }
                }
            }
        }

        private MemoryTransferData PerformMemoryTransfer( ImageInfo imageInfo, ref TwainResult result )
        {
            /*
            ImageLayout imageLayout = new ImageLayout();
            result = Twain32Native.DsImageLayout(
                                    ApplicationId,
                                    DataSource.SourceId,
                                    DataGroup.Image,
                                    DataArgumentType.ImageLayout,
                                    Message.Get,
                                    imageLayout );

            if( result != TwainResult.Success )
            {
                DataSource.Close();
                return null;
            }
            */

            // Setup incremental Memory XFer                                        
            SetupMemXfer setupMemXfer = new SetupMemXfer();
            result = Twain32Native.DsSetupMemXfer(
                ApplicationId,
                DataSource.SourceId,
                DataGroup.Control,
                DataArgumentType.SetupMemXfer,
                Message.Get,
                setupMemXfer
                );

            if( result != TwainResult.Success )
            {
                return null;
            }

            // allocate the preferred buffer size                    
            ImageMemXfer imageMemXfer = new ImageMemXfer();
            try
            {
                imageMemXfer.Memory.Flags = MemoryFlags.AppOwns | MemoryFlags.Pointer;
                imageMemXfer.Memory.Length = setupMemXfer.Preferred;
                imageMemXfer.Memory.TheMem = Kernel32Native.GlobalAlloc( GlobalAllocFlags.MemFixed, ( int )setupMemXfer.Preferred );
                if( imageMemXfer.Memory.TheMem == IntPtr.Zero )
                {
                    throw new TwainException( "error allocating buffer for memory transfer" );
                }
                IntPtr orgTheMem = imageMemXfer.Memory.TheMem;

                using( System.IO.MemoryStream ms = new System.IO.MemoryStream() )
                {
                    do
                    {
                        // perform a transfer
                        result = Twain32Native.DsImageMemXfer(
                            ApplicationId,
                            DataSource.SourceId,
                            DataGroup.Image,
                            DataArgumentType.ImageMemXfer,
                            Message.Get,
                            imageMemXfer
                            );

                        switch( result )
                        {
                            case TwainResult.Success:
                            case TwainResult.XferDone:
                                DataSource.State = 7;
                                byte[] data = new byte[imageMemXfer.BytesWritten];
                                Marshal.Copy( imageMemXfer.Memory.TheMem, data, 0, ( int )imageMemXfer.BytesWritten );
                                ms.Write( data, 0, ( int )imageMemXfer.BytesWritten );
                                break;

                            case TwainResult.Cancel:
                                DataSource.State = 7;
                                break;

                            default:
                                ConditionCode conditionCode = GetConditionCode( ApplicationId, DataSource.SourceId );
                                throw new TwainException( $"ImageNativeXfer failed. {result}, {conditionCode}", result, conditionCode );
                        }

                    } while( result == TwainResult.Success );

                    if( result != TwainResult.XferDone )
                    {
                        return null;
                    }

                    return new MemoryTransferData
                    {
                        Data = ms.ToArray(),
                        ImageInfo = imageInfo,
                        ImageMemXfer = imageMemXfer,
                    };
                }
            }
            finally
            {
                if( imageMemXfer.Memory.TheMem != IntPtr.Zero )
                {
                    Kernel32Native.GlobalFree( imageMemXfer.Memory.TheMem );
                    imageMemXfer.Memory.TheMem = IntPtr.Zero;
                }
            }
        }

        protected void CloseDsAndCompleteScanning( Exception exception )
        {
            EndingScan();
            DataSource.Close();
            try
            {
                ScanningComplete( this, new ScanningCompleteEventArgs( exception ) );
            }
            catch
            {
            }
        }

        protected void EndingScan()
        {
            _messageHook.UseFilter = false;
        }

        public void SelectSource()
        {
            DataSource.Dispose();
            DataSource = DataSource.UserSelected( ApplicationId, _messageHook );
        }

        public void SelectSource( DataSource dataSource )
        {
            DataSource.Dispose();
            DataSource = dataSource;
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool disposing )
        {
            Marshal.FreeHGlobal( _eventMessage.EventPtr );

            if( disposing )
            {
                DataSource.Dispose();

                IntPtr windowHandle = _messageHook.WindowHandle;

                if( ApplicationId.Id != 0 )
                {
                    // Close down the data source manager
                    Twain32Native.DsmParent(
                        ApplicationId,
                        IntPtr.Zero,
                        DataGroup.Control,
                        DataArgumentType.Parent,
                        Message.CloseDSM,
                        ref windowHandle );
                }

                ApplicationId.Id = 0;
            }
        }

        public static ConditionCode GetConditionCode( Identity applicationId, Identity sourceId )
        {
            Status status = new Status();

            Twain32Native.DsmStatus(
                applicationId,
                sourceId,
                DataGroup.Control,
                DataArgumentType.Status,
                Message.Get,
                status );

            return status.ConditionCode;
        }

        public static readonly Identity DefaultApplicationId = new Identity()
        {
            Id = BitConverter.ToInt32( Guid.NewGuid().ToByteArray(), 0 ),
            Version = new TwainVersion()
            {
                MajorNum = 1,
                MinorNum = 1,
                Language = Language.USA,
                Country = Country.USA,
                Info = Assembly.GetExecutingAssembly().FullName
            },
            ProtocolMajor = TwainConstants.ProtocolMajor,
            ProtocolMinor = TwainConstants.ProtocolMinor,
            SupportedGroups = ( int )( DataGroup.Image | DataGroup.Control ),
            Manufacturer = "TwainDotNet",
            ProductFamily = "TwainDotNet",
            ProductName = "TwainDotNet",
        };
    }
}
