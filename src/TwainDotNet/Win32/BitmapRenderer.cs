using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using log4net;

namespace TwainDotNet.Win32
{
	/// <summary>
	/// Bitmap renderer.
	/// </summary>
    public class BitmapRenderer : IDisposable
    {
        /// <summary>
        /// The logger for this class.
        /// </summary>
        static readonly ILog log = LogManager.GetLogger(typeof(BitmapRenderer));

        readonly IntPtr _dibHandle;
        readonly IntPtr _bitmapPointer;
        readonly IntPtr _pixelInfoPointer;
        Rectangle _rectangle;
        readonly BitmapInfoHeader _bitmapInfo;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="dibHandle">The dib handle.</param>
        public BitmapRenderer(IntPtr dibHandle)
        {
            _dibHandle = dibHandle;
            _bitmapPointer = Kernel32Native.GlobalLock(dibHandle);

            _bitmapInfo = new BitmapInfoHeader();
            Marshal.PtrToStructure(_bitmapPointer, _bitmapInfo);
            log.Debug(_bitmapInfo.ToString());

            _rectangle = new Rectangle();
            _rectangle.X = _rectangle.Y = 0;
            _rectangle.Width = _bitmapInfo.Width;
            _rectangle.Height = _bitmapInfo.Height;

            if (_bitmapInfo.SizeImage == 0)
            {
                _bitmapInfo.SizeImage = ((((_bitmapInfo.Width * _bitmapInfo.BitCount) + 31) & ~31) >> 3) * _bitmapInfo.Height;
            }


            // The following code only works on x86
            Debug.Assert(Marshal.SizeOf(typeof(IntPtr)) == 4);

            int pixelInfoPointer = _bitmapInfo.ClrUsed;
            if ((pixelInfoPointer == 0) && (_bitmapInfo.BitCount <= 8))
            {
                pixelInfoPointer = 1 << _bitmapInfo.BitCount;
            }

            pixelInfoPointer = (pixelInfoPointer * 4) + _bitmapInfo.Size + _bitmapPointer.ToInt32();

            _pixelInfoPointer = new IntPtr(pixelInfoPointer);
        }

		/// <summary>
		/// Finalizer.
		/// </summary>
        ~BitmapRenderer()
        {
            Dispose(false);
        }

		/// <summary>
		/// Render to bitmap.
		/// </summary>
		/// <returns></returns>
        public Bitmap RenderToBitmap()
        {
            Bitmap bitmap = new Bitmap(_rectangle.Width, _rectangle.Height);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                IntPtr hdc = graphics.GetHdc();

                try
                {
                    Gdi32Native.SetDIBitsToDevice(hdc, 0, 0, _rectangle.Width, _rectangle.Height,
                        0, 0, 0, _rectangle.Height, _pixelInfoPointer, _bitmapPointer, 0);
                }
                finally
                {
                    graphics.ReleaseHdc(hdc);
                }
            }

            bitmap.SetResolution(PpmToDpi(_bitmapInfo.XPelsPerMeter), PpmToDpi(_bitmapInfo.YPelsPerMeter));

            return bitmap;
        }

        private static float PpmToDpi(double pixelsPerMeter)
        {
            double pixelsPerMillimeter = (double)pixelsPerMeter / 1000.0;
            double dotsPerInch = pixelsPerMillimeter * 25.4;
            return (float)Math.Round(dotsPerInch, 2);
        }

		/// <summary>
		/// Dispose.
		/// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

		/// <summary>
		/// Dispose.
		/// </summary>
		/// <param name="disposing">Whether disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            Kernel32Native.GlobalUnlock(_dibHandle);
            Kernel32Native.GlobalFree(_dibHandle);
        }
    }
}
