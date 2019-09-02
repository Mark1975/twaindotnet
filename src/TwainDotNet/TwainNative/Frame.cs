using System.Runtime.InteropServices;

namespace TwainDotNet.TwainNative
{
    // all values are in Inches (2.54 cm)
	/// <summary>
	/// The frame.
	/// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class Frame
    {
		/// <summary>
		/// The left.
		/// </summary>
        public Fix32 Left;

		/// <summary>
		/// The top.
		/// </summary>
        public Fix32 Top;

		/// <summary>
		/// The right.
		/// </summary>
        public Fix32 Right;

		/// <summary>
		/// The bottom.
		/// </summary>
        public Fix32 Bottom;

		/// <summary>
		/// Overrides ToString().
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(
				"l {0} t {1} r {2} b {3}",
				Left,
				Top,
				Right,
				Bottom );
		}
	}
}