using System;
using System.Runtime.InteropServices;
using TwainDotNet.Win32;

namespace TwainDotNet.TwainNative
{
    /// <summary>
    /// /* DAT_CAPABILITY. Used by application to get/set capability from/in a data source. */
    /// typedef struct {
    ///    TW_UINT16  Cap; /* id of capability to set or get, e.g. CAP_BRIGHTNESS */
    ///    TW_UINT16  ConType; /* TWON_ONEVALUE, _RANGE, _ENUMERATION or _ARRAY   */
    ///    TW_HANDLE  hContainer; /* Handle to container of type Dat              */
    /// } TW_CAPABILITY, FAR * pTW_CAPABILITY;
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 2 )]
    public class TwainCapability : IDisposable
    {
        Capabilities _capabilities;
        ContainerType _containerType;
        IntPtr _handle;

        public TwainCapability( Capabilities capabilities )
        {
            _capabilities = capabilities;
        }

        public CapabilityResult ReadValue()
        {
            if( _handle == IntPtr.Zero )
            {
                throw new TwainException( "No handle available for reading." );
            }

            uint size = Kernel32Native.GlobalSize( _handle );
            IntPtr p = Kernel32Native.GlobalLock( _handle );
            try
            {
                switch( _containerType )
                {
                    case ContainerType.Array:
                        return ArrayCapabilityResult.FromPointer( p );

                    case ContainerType.Enum:
                        return EnumCapabilityResult.FromPointer( p );

                    case ContainerType.One:
                        return BasicCapabilityResult.FromPointer( p, size );

                    case ContainerType.Range:
                        return RangeCapabilityResult.FromPointer( p );

                    default:
                        throw new NotImplementedException();
                }
            }
            finally
            {
                Kernel32Native.GlobalUnlock( _handle );
            }
        }

        public void WriteValue( CapabilityOneValue value )
        {
            if( _handle != IntPtr.Zero )
            {
                throw new TwainException( "Handle is already assigned." );
            }

            _containerType = ContainerType.One;

            _handle = Kernel32Native.GlobalAlloc( GlobalAllocFlags.Handle, 6 );
            IntPtr p = Kernel32Native.GlobalLock( _handle );
            try
            {
                Marshal.WriteInt16( p, 0, ( short )value.TwainType );
                Marshal.WriteInt32( p, 2, value.Value );
            }
            finally
            {
                Kernel32Native.GlobalUnlock( _handle );
            }
        }

        public void Dispose()
        {
            Dispose( true );
        }

        protected virtual void Dispose( bool disposing )
        {
            if( _handle != IntPtr.Zero )
            {
                Kernel32Native.GlobalFree( _handle );
                _handle = IntPtr.Zero;
            }
        }
    }
}
