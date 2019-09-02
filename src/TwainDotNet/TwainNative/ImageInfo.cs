using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TwainDotNet.TwainNative
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
#pragma warning disable 1591
	public class ImageInfo
    {
        public Fix32 XResolution;
        public Fix32 YResolution;
        public int ImageWidth;
        public int ImageLength;
        public short SamplesPerPixel;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public short[] BitsPerSample;
        public short BitsPerPixel;
        public short Planar;
        public PixelType PixelType;
        public Compression Compression;

		public override string ToString()
		{
			return string.Format(
				"ImageInfo xres:{0} yres:{1} w:{2} h:{3} spp:{4} bps:{5} bpp:{6} planar:{7} pixelType:{8} compression:{9}",
				XResolution.ToFloat(),
				YResolution.ToFloat(),
				ImageWidth,
				ImageLength,
				SamplesPerPixel,
				BitsPerSample,
				BitsPerPixel,
				Planar,
				PixelType,
				Compression );
		}
	}
#pragma warning restore 1591
}
