using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TwainDotNet.TwainNative;

namespace TwainDotNet
{
    public abstract class CapabilityResult
    {
        public TwainType TwainType
        {
            get; set;
        }

        protected byte[] RawBytes
        {
            get; set;
        }

        protected ushort GetUInt16Item( int index )
        {
            if( this.RawBytes == null )
            {
                throw new InvalidOperationException();
            }

            int dataSize = GetDataSize( this.TwainType );
            return BitConverter.ToUInt16( this.RawBytes, index * dataSize );
        }

        protected Fix32 GetFix32Item( int index )
        {
            if( this.RawBytes == null )
            {
                throw new InvalidOperationException();
            }

            int dataSize = GetDataSize( this.TwainType );
            return new Fix32( BitConverter.ToInt16( this.RawBytes, index * dataSize ), BitConverter.ToUInt16( this.RawBytes, index * dataSize + 2 ) );
        }

        protected IEnumerable<T> GetItems<T>( int itemCount, Func<int, T> getItem )
        {
            var result = new List<T>();
            for( int idx = 0; idx < itemCount; idx++ )
            {
                result.Add( getItem( idx ) );
            }
            return result;
        }

        protected static int GetDataSize( TwainType twainType )
        {
            switch( twainType )
            {
                case TwainType.Int8:
                case TwainType.UInt8:
                    return 1;

                case TwainType.Bool:
                case TwainType.Int16:
                case TwainType.UInt16:
                    return 2;

                case TwainType.Int32:
                case TwainType.UInt32:
                case TwainType.Fix32:
                case TwainType.Handle:
                    return 4;

                case TwainType.Frame: // 4 * Fix32
                    return 16;

                case TwainType.Str32:
                    return 34;

                case TwainType.Str64:
                    return 66;

                case TwainType.Str128:
                    return 130;

                case TwainType.Str255:
                    return 256;

                default:
                    throw new NotSupportedException();
            }
        }
    }

    public class BasicCapabilityResult : CapabilityResult
    {
        public int RawBasicValue { get; set; }

        public bool BoolValue { get { return RawBasicValue == 1; } }

        public short Int16Value { get { return (short)RawBasicValue; } }

        public ushort UInt16Value
        {
            get
            {
                return ( ushort )RawBasicValue;
            }
        }

        public int Int32Value { get { return RawBasicValue; } }

        public string StringValue
        {
            get; set;
        }

        public static BasicCapabilityResult FromPointer( IntPtr p, uint size )
        {
            BasicCapabilityResult result = new BasicCapabilityResult();

            result.TwainType = ( TwainType )Marshal.ReadInt16( p );
            result.RawBasicValue = Marshal.ReadInt32( p, 2 );

            switch( result.TwainType )
            {
                case TwainType.Str32:
                case TwainType.Str64:
                case TwainType.Str128:
                case TwainType.Str255:
                    byte[] rawData = new byte[size];
                    Marshal.Copy( p, rawData, 0, ( int )size );
                    result.StringValue = System.Text.Encoding.Default.GetString( rawData, 2, (int)size - 2 );
                    break;
                default:
                    break;
            }
            return result;
        }
    }

    public class EnumCapabilityResult : CapabilityResult
    {
        public int ItemCount
        {
            get; set;
        }

        public int CurrentIndex
        {
            get; set;
        }
        public int DefaultIndex
        {
            get; set;
        }

        public static EnumCapabilityResult FromPointer( IntPtr p )
        {
            EnumCapabilityResult result = new EnumCapabilityResult();

            result.TwainType = ( TwainType )Marshal.ReadInt16( p );
            result.ItemCount = Marshal.ReadInt32( p, 2 );
            result.CurrentIndex = Marshal.ReadInt32( p, 6 );
            result.DefaultIndex = Marshal.ReadInt32( p, 10 );

            int rawDataSize = GetDataSize( result.TwainType ) * result.ItemCount;
            result.RawBytes = new byte[rawDataSize];
            Marshal.Copy( new IntPtr( p.ToInt32() + 14 ), result.RawBytes, 0, rawDataSize );
            return result;
        }

        public IEnumerable<ushort> GetUInt16Items()
        {
            return this.GetItems( this.ItemCount, this.GetUInt16Item );
        }

        public IEnumerable<Fix32> GetFix32Items()
        {
            return this.GetItems( this.ItemCount, this.GetFix32Item );
        }
    }

    public class ArrayCapabilityResult : CapabilityResult
    {
        public int ItemCount
        {
            get; set;
        }

        public static ArrayCapabilityResult FromPointer( IntPtr p )
        {
            ArrayCapabilityResult result = new ArrayCapabilityResult();
            result.TwainType = ( TwainType )Marshal.ReadInt16( p );
            result.ItemCount = Marshal.ReadInt32( p, 2 );

            int rawDataSize = GetDataSize( result.TwainType ) * result.ItemCount;
            result.RawBytes = new byte[rawDataSize];
            Marshal.Copy( new IntPtr(p.ToInt32() + 6), result.RawBytes, 0, rawDataSize );
            return result;
        }

        public IEnumerable<ushort> GetUInt16Items()
        {
            return this.GetItems( this.ItemCount, this.GetUInt16Item );
        }

        public IEnumerable<Fix32> GetFix32Items()
        {
            return this.GetItems( this.ItemCount, this.GetFix32Item );
        }
    }

    public class RangeCapabilityResult : CapabilityResult
    {
        public uint MinValue
        {
            get; set;
        }

        public uint MaxValue
        {
            get; set;
        }

        public uint StepSize
        {
            get; set;
        }

        public uint DefaultValue
        {
            get; set;
        }

        public uint CurrentValue
        {
            get; set;
        }

        public static RangeCapabilityResult FromPointer( IntPtr p )
        {
            RangeCapabilityResult result = new RangeCapabilityResult();
            result.TwainType = ( TwainType )Marshal.ReadInt16( p );
            byte[] rawBytes = new byte[22];
            Marshal.Copy( p, rawBytes, 0, 22 );

            result.MinValue = BitConverter.ToUInt32( rawBytes, 2 );
            result.MaxValue = BitConverter.ToUInt32( rawBytes, 6 );
            result.StepSize = BitConverter.ToUInt32( rawBytes, 10 );
            result.DefaultValue = BitConverter.ToUInt32( rawBytes, 14 );
            result.CurrentValue = BitConverter.ToUInt32( rawBytes, 18 );
            return result;
        }
    }
}
