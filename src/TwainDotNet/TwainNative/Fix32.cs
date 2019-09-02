using System;
using System.Runtime.InteropServices;

namespace TwainDotNet.TwainNative
{
	/// <summary>
	/// Fix 32.
	/// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class Fix32
    {
		/// <summary>
		/// The whole.
		/// </summary>
        public short Whole;

		/// <summary>
		/// The frac.
		/// </summary>
        public ushort Frac;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="f">The f.</param>
        public Fix32(float f)
        {
            // http://www.dosadi.com/forums/archive/index.php?t-2534.html
            var val = (int)(f * 65536.0F);
            this.Whole = Convert.ToInt16(val >> 16);    // most significant 16 bits
            this.Frac = Convert.ToUInt16(val & 0xFFFF); // least
        }

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="whole">The whole.</param>
		/// <param name="frac">The frac.</param>
        public Fix32( short whole, ushort frac )
        {
            this.Whole = whole;
            this.Frac = frac;
        }

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="value">The value.</param>
		public Fix32( int value )
		{
			byte[] bytes = BitConverter.GetBytes( value );
			this.Whole = BitConverter.ToInt16( bytes, 0 );
			this.Frac = BitConverter.ToUInt16( bytes, 2 );
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="value">The value.</param>
		public Fix32( uint value )
		{
			byte[] bytes = BitConverter.GetBytes( value );
			this.Whole = BitConverter.ToInt16( bytes, 0 );
			this.Frac = BitConverter.ToUInt16( bytes, 2 );
		}

		/// <summary>
		/// To float.
		/// </summary>
		/// <returns></returns>
		public float ToFloat()
        {
            var frac = Convert.ToSingle(this.Frac);
            return this.Whole + frac / 65536.0F;
        }

		/// <summary>
		/// To string.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.ToFloat().ToString();
		}
	}
}