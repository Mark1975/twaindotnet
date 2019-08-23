using System.Runtime.InteropServices;

namespace TwainDotNet.TwainNative
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class ImageLayout
    {
        public Frame Frame;

        public uint DocumentNumber;

        public uint PageNumber;

        public uint FrameNumber;

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