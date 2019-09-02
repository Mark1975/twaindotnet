using System.Runtime.InteropServices;

namespace TwainDotNet.TwainNative
{
	/// <summary>
	/// Image layout.
	/// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class ImageLayout
    {
		/// <summary>
		/// The frame.
		/// </summary>
        public Frame Frame;

		/// <summary>
		/// The document number.
		/// </summary>
        public uint DocumentNumber;

		/// <summary>
		/// The page number.
		/// </summary>
        public uint PageNumber;

		/// <summary>
		/// The frame number.
		/// </summary>
        public uint FrameNumber;

		/// <summary>
		/// Overrides ToString().
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(
				"ImageLayout Frame:{0} DocumentNumber:{1} PageNumber:{2} FrameNumber:{3}",
				Frame,
				DocumentNumber,
				PageNumber,
				FrameNumber );
		}
	}
}