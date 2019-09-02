using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TwainDotNet.TwainNative;

namespace TwainDotNet
{
	internal abstract class CapabilityResult
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

		protected short GetInt16Item( int index )
		{
			if( this.RawBytes == null )
			{
				throw new InvalidOperationException();
			}

			int dataSize = GetDataSize( this.TwainType );
			return BitConverter.ToInt16( this.RawBytes, index * dataSize );
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

	internal class BasicCapabilityResult : CapabilityResult
	{
		public int RawBasicValue
		{
			get; set;
		}

		public bool BoolValue
		{
			get
			{
				return RawBasicValue == 1;
			}
		}

		public short Int16Value
		{
			get
			{
				return ( short )RawBasicValue;
			}
		}

		public ushort UInt16Value
		{
			get
			{
				return ( ushort )RawBasicValue;
			}
		}

		public int Int32Value
		{
			get
			{
				return RawBasicValue;
			}
		}

		public Fix32 Fix32Value
		{
			get
			{
				return new Fix32( RawBasicValue );
			}
		}

		public string StringValue
		{
			get; set;
		}

		public static BasicCapabilityResult FromPointer( IntPtr p, uint size )
		{
			BasicCapabilityResult result = new BasicCapabilityResult
			{
				TwainType = ( TwainType )Marshal.ReadInt16( p ),
				RawBasicValue = Marshal.ReadInt32( p, 2 ),
			};

			switch( result.TwainType )
			{
				case TwainType.Str32:
				case TwainType.Str64:
				case TwainType.Str128:
				case TwainType.Str255:
					byte[] rawData = new byte[size];
					Marshal.Copy( p, rawData, 0, ( int )size );
					result.StringValue = System.Text.Encoding.Default.GetString( rawData, 2, ( int )size - 2 );
					break;
				default:
					break;
			}
			return result;
		}

		public override string ToString()
		{
			string value;
			switch( this.TwainType )
			{
				case TwainType.Fix32:
					value = this.Fix32Value.ToString();
					break;

				default:
					value = this.RawBasicValue.ToString();
					break;
			}
			return $"{TwainType} {value}";
		}
	}

	internal class EnumCapabilityResult : CapabilityResult
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
			EnumCapabilityResult result = new EnumCapabilityResult
			{
				TwainType = ( TwainType )Marshal.ReadInt16( p ),
				ItemCount = Marshal.ReadInt32( p, 2 ),
				CurrentIndex = Marshal.ReadInt32( p, 6 ),
				DefaultIndex = Marshal.ReadInt32( p, 10 ),
			};

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

		public override string ToString()
		{
			string values = "";
			for( int i = 0; i < ItemCount; i++ )
			{
				if( i > 0 )
				{
					values += ", ";
				}

				switch( this.TwainType )
				{
					case TwainType.UInt16:
						values += this.GetUInt16Item( i ).ToString();
						break;
					case TwainType.Int16:
						values += this.GetInt16Item( i ).ToString();
						break;
					case TwainType.Fix32:
						values += this.GetFix32Item( i ).ToString();
						break;

				}
			}
			return $"{TwainType} ItemCount: {ItemCount} CurrentIndex: {CurrentIndex} DefaultIndex: {DefaultIndex} Values: {values}";
		}
	}

	internal class ArrayCapabilityResult : CapabilityResult
	{
		public int ItemCount
		{
			get; set;
		}

		public static ArrayCapabilityResult FromPointer( IntPtr p )
		{
			ArrayCapabilityResult result = new ArrayCapabilityResult
			{
				TwainType = ( TwainType )Marshal.ReadInt16( p ),
				ItemCount = Marshal.ReadInt32( p, 2 ),
			};

			int rawDataSize = GetDataSize( result.TwainType ) * result.ItemCount;
			result.RawBytes = new byte[rawDataSize];
			Marshal.Copy( new IntPtr( p.ToInt32() + 6 ), result.RawBytes, 0, rawDataSize );
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

	internal class RangeCapabilityResult : CapabilityResult
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
			byte[] rawBytes = new byte[22];
			Marshal.Copy( p, rawBytes, 0, 22 );

			RangeCapabilityResult result = new RangeCapabilityResult
			{
				TwainType = ( TwainType )Marshal.ReadInt16( p ),
				MinValue = BitConverter.ToUInt32( rawBytes, 2 ),
				MaxValue = BitConverter.ToUInt32( rawBytes, 6 ),
				StepSize = BitConverter.ToUInt32( rawBytes, 10 ),
				DefaultValue = BitConverter.ToUInt32( rawBytes, 14 ),
				CurrentValue = BitConverter.ToUInt32( rawBytes, 18 ),
			};

			return result;
		}

		public override string ToString()
		{
			string minValue = this.MinValue.ToString();
			string maxValue = this.MaxValue.ToString();
			string stepSize = this.StepSize.ToString();
			string defaultValue = this.DefaultValue.ToString();
			string currentValue = this.CurrentValue.ToString();

			switch( this.TwainType )
			{
				case TwainType.Fix32:
					minValue = new Fix32( this.MinValue ).ToString();
					maxValue = new Fix32( this.MaxValue ).ToString();
					stepSize = new Fix32( this.StepSize ).ToString();
					defaultValue = new Fix32( this.DefaultValue ).ToString();
					currentValue = new Fix32( this.CurrentValue ).ToString();
					break;

			}

			return $"{TwainType} MinValue: {minValue} MaxValue: {maxValue} StepSize: {stepSize} DefaultValue: {defaultValue} CurrentValue: {currentValue}";
		}
	}
}
