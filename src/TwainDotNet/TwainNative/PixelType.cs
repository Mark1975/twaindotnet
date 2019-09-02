using System;
using System.Collections.Generic;
using System.Text;

namespace TwainDotNet.TwainNative
{
#pragma warning disable 1591
	public enum PixelType : ushort
    {
        BlackAndWhite = 0,
        Grey = 1,
        Rgb = 2,
        Palette = 3,
        Cmy = 4,
        Cmyk = 5,
        Yuv = 6,
        Yuvk = 7,
        CieXyz = 8,
        Lab = 9,
        Srgb = 10,
        Scrbg = 11,
        Infrared = 16
    }
#pragma warning restore 1591
}
