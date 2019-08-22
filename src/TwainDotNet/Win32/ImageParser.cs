using System;
using System.IO;
using System.Runtime.InteropServices;
using TwainDotNet.TwainNative;

namespace TwainDotNet.Win32
{
    public static class ImageParser
    {
        public static System.Drawing.Image ParseImage( MemoryTransferData memoryTransferData )
        {
            if( memoryTransferData == null )
            {
                throw new ArgumentNullException( nameof( memoryTransferData ) );
            }

            if( memoryTransferData.ImageInfo == null )
            {
                throw new ArgumentException( "memoryTransferData must contain ImageInfo.", nameof( memoryTransferData ) );
            }

            if( memoryTransferData.ImageMemXfer == null )
            {
                throw new ArgumentException( "memoryTransferData must contain ImageMemXfer.", nameof( memoryTransferData ) );
            }

            if( memoryTransferData.Data == null || memoryTransferData.Data.Length == 0 )
            {
                throw new ArgumentException( "memoryTransferData must contain Data.", nameof( memoryTransferData ) );
            }

            ImageInfo imageinfo = memoryTransferData.ImageInfo;

            if( imageinfo.BitsPerPixel == 24 )
            {
                return ParseUncompressedColorImage( memoryTransferData );
            }

            if( imageinfo.BitsPerPixel == 8 || imageinfo.BitsPerPixel == 16 )
            {
                return ParseUncompressedGrayscaleImage( memoryTransferData );
            }

            throw new NotSupportedException( $"BitsPerPixel={imageinfo.BitsPerPixel} is not supported." );
        }

        private static System.Drawing.Image ParseUncompressedGrayscaleImage( MemoryTransferData memoryTransferData )
        {
            ImageInfo imageinfo = memoryTransferData.ImageInfo;
            ImageMemXfer imageMemXfer = memoryTransferData.ImageMemXfer;

            const int iSpaceForHeader = 512;

            byte[] tiffHeader = new byte[iSpaceForHeader];

            // Each row contains padding bytes; but I haven't found a way to inject that information in the Tiff header.
            // Workaround: remove the padding bytes.
            byte[] data = RemoveRowPadding( memoryTransferData.Data, imageMemXfer.BytesPerRow, imageMemXfer.Columns, imageinfo.BitsPerPixel / 8, imageinfo.ImageLength );

            TiffGrayscaleUncompressed tiffgrayscaleuncompressed = new TiffGrayscaleUncompressed( ( uint )imageinfo.ImageWidth, ( uint )imageinfo.ImageLength, ( uint )imageinfo.XResolution, ( uint )data.Length, ( uint )imageinfo.BitsPerPixel );

            // Create memory for the TIFF header...
            IntPtr intptrTiff = Marshal.AllocHGlobal( Marshal.SizeOf( tiffgrayscaleuncompressed ) );
            try
            {
                // Copy the header into the memory...
                Marshal.StructureToPtr( tiffgrayscaleuncompressed, intptrTiff, true );

                // Copy the memory into the byte array (we left room for it), giving us a
                // TIFF image starting at (iSpaceForHeader - Marshal.SizeOf(tiffbitonal))
                // in the byte array...
                Marshal.Copy( intptrTiff, tiffHeader, iSpaceForHeader - Marshal.SizeOf( tiffgrayscaleuncompressed ), Marshal.SizeOf( tiffgrayscaleuncompressed ) );

                using( MemoryStream ms = new MemoryStream() )
                {
                    ms.Write
                    (
                        tiffHeader,
                        iSpaceForHeader - Marshal.SizeOf( tiffgrayscaleuncompressed ),
                        tiffHeader.Length - ( iSpaceForHeader - Marshal.SizeOf( tiffgrayscaleuncompressed ) )
                    );

                    ms.Write( data, 0, data.Length );

                    return System.Drawing.Image.FromStream( ms );
                }
            }
            finally
            {
                // Free the memory...
                Marshal.FreeHGlobal( intptrTiff );
                intptrTiff = IntPtr.Zero;
            }
        }

        private static byte[] RemoveRowPadding( byte[] data, uint bytesPerRow, uint columns, int bytesPerPixel, int imageLength )
        {
            if( bytesPerRow == columns * bytesPerPixel )
            {
                return data;
            }

            using( MemoryStream ms = new MemoryStream() )
            {
                for( int row = 0; row < imageLength; row++ )
                {
                    ms.Write( data, row * ( int )bytesPerRow, ( int )columns * bytesPerPixel );
                }
                return ms.ToArray();
            }
        }

        private static System.Drawing.Image ParseUncompressedColorImage( MemoryTransferData memoryTransferData )
        {
            ImageInfo imageinfo = memoryTransferData.ImageInfo;
            ImageMemXfer imageMemXfer = memoryTransferData.ImageMemXfer;

            const int iSpaceForHeader = 512;

            byte[] tiffHeader = new byte[iSpaceForHeader];

            // Each row contains padding bytes; but I haven't found a way to inject that information in the Tiff header.
            // Workaround: remove the padding bytes.
            byte[] data = RemoveRowPadding( memoryTransferData.Data, imageMemXfer.BytesPerRow, imageMemXfer.Columns, imageinfo.BitsPerPixel / 8, imageinfo.ImageLength );

            TiffColorUncompressed tiffcoloruncompressed = new TiffColorUncompressed( ( uint )imageinfo.ImageWidth, ( uint )imageinfo.ImageLength, ( uint )imageinfo.XResolution, ( uint )data.Length );

            // Create memory for the TIFF header...
            IntPtr intptrTiff = Marshal.AllocHGlobal( Marshal.SizeOf( tiffcoloruncompressed ) );
            try
            {
                // Copy the header into the memory...
                Marshal.StructureToPtr( tiffcoloruncompressed, intptrTiff, true );

                // Copy the memory into the byte array (we left room for it), giving us a
                // TIFF image starting at (iSpaceForHeader - Marshal.SizeOf(tiffbitonal))
                // in the byte array...
                Marshal.Copy( intptrTiff, tiffHeader, iSpaceForHeader - Marshal.SizeOf( tiffcoloruncompressed ), Marshal.SizeOf( tiffcoloruncompressed ) );

                using( MemoryStream ms = new MemoryStream() )
                {
                    ms.Write
                    (
                        tiffHeader,
                        iSpaceForHeader - Marshal.SizeOf( tiffcoloruncompressed ),
                        tiffHeader.Length - ( iSpaceForHeader - Marshal.SizeOf( tiffcoloruncompressed ) )
                    );

                    ms.Write( data, 0, data.Length );

                    return System.Drawing.Image.FromStream( ms );
                }
            }
            finally
            {
                // Free the memory...
                Marshal.FreeHGlobal( intptrTiff );
                intptrTiff = IntPtr.Zero;
            }
        }

        // A TIFF header is composed of tags...
        [StructLayout( LayoutKind.Sequential, Pack = 2 )]
        private struct TiffTag
        {
            public TiffTag( ushort a_u16Tag, ushort a_u16Type, uint a_u32Count, uint a_u32Value )
            {
                u16Tag = a_u16Tag;
                u16Type = a_u16Type;
                u32Count = a_u32Count;
                u32Value = a_u32Value;
            }

            public ushort u16Tag;
            public ushort u16Type;
            public uint u32Count;
            public uint u32Value;
        }

        // TIFF header for Uncompressed COLOR images...
        [StructLayout( LayoutKind.Sequential, Pack = 2 )]
        private struct TiffColorUncompressed
        {
            // Constructor...
            public TiffColorUncompressed( uint a_u32Width, uint a_u32Height, uint a_u32Resolution, uint a_u32Size )
            {
                // Header...
                u16ByteOrder = 0x4949;
                u16Version = 42;
                u32OffsetFirstIFD = 8;

                // First IFD...
                u16IFD = 14;

                // Tags...
                tifftagNewSubFileType = new TiffTag( 254, 4, 1, 0 );
                tifftagSubFileType = new TiffTag( 255, 3, 1, 1 );
                tifftagImageWidth = new TiffTag( 256, 4, 1, a_u32Width );
                tifftagImageLength = new TiffTag( 257, 4, 1, a_u32Height );
                tifftagBitsPerSample = new TiffTag( 258, 3, 3, 182 );
                tifftagCompression = new TiffTag( 259, 3, 1, 1 );
                tifftagPhotometricInterpretation = new TiffTag( 262, 3, 1, 2 );
                tifftagStripOffsets = new TiffTag( 273, 4, 1, 204 );
                tifftagSamplesPerPixel = new TiffTag( 277, 3, 1, 3 );
                tifftagRowsPerStrip = new TiffTag( 278, 4, 1, a_u32Height );
                tifftagStripByteCounts = new TiffTag( 279, 4, 1, a_u32Size );
                tifftagXResolution = new TiffTag( 282, 5, 1, 188 );
                tifftagYResolution = new TiffTag( 283, 5, 1, 196 );
                tifftagResolutionUnit = new TiffTag( 296, 3, 1, 2 );

                // Footer...
                u32NextIFD = 0;
                u16XBitsPerSample1 = 8;
                u16XBitsPerSample2 = 8;
                u16XBitsPerSample3 = 8;
                u64XResolution = ( ulong )0x100000000 + ( ulong )a_u32Resolution;
                u64YResolution = ( ulong )0x100000000 + ( ulong )a_u32Resolution;
            }

            // Header...
            public ushort u16ByteOrder;
            public ushort u16Version;
            public uint u32OffsetFirstIFD;

            // First IFD...
            public ushort u16IFD;

            // Tags...
            public TiffTag tifftagNewSubFileType;
            public TiffTag tifftagSubFileType;
            public TiffTag tifftagImageWidth;
            public TiffTag tifftagImageLength;
            public TiffTag tifftagBitsPerSample;
            public TiffTag tifftagCompression;
            public TiffTag tifftagPhotometricInterpretation;
            public TiffTag tifftagStripOffsets;
            public TiffTag tifftagSamplesPerPixel;
            public TiffTag tifftagRowsPerStrip;
            public TiffTag tifftagStripByteCounts;
            public TiffTag tifftagXResolution;
            public TiffTag tifftagYResolution;
            public TiffTag tifftagResolutionUnit;

            // Footer...
            public uint u32NextIFD;
            public ushort u16XBitsPerSample1;
            public ushort u16XBitsPerSample2;
            public ushort u16XBitsPerSample3;
            public ulong u64XResolution;
            public ulong u64YResolution;
        }

        // TIFF header for Uncompressed GRAYSCALE images...
        [StructLayout( LayoutKind.Sequential, Pack = 2 )]
        private struct TiffGrayscaleUncompressed
        {
            // Constructor...
            public TiffGrayscaleUncompressed( uint a_u32Width, uint a_u32Height, uint a_u32Resolution, uint a_u32Size, uint bitsPerPixel )
            {
                // Header...
                u16ByteOrder = 0x4949;
                u16Version = 42;
                u32OffsetFirstIFD = 8;

                // First IFD...
                u16IFD = 14;

                // Tags...
                tifftagNewSubFileType = new TiffTag( 254, 4, 1, 0 );
                tifftagSubFileType = new TiffTag( 255, 3, 1, 1 );
                tifftagImageWidth = new TiffTag( 256, 4, 1, a_u32Width );
                tifftagImageLength = new TiffTag( 257, 4, 1, a_u32Height );
                tifftagBitsPerSample = new TiffTag( 258, 3, 1, bitsPerPixel );
                tifftagCompression = new TiffTag( 259, 3, 1, 1 );
                tifftagPhotometricInterpretation = new TiffTag( 262, 3, 1, 1 );
                tifftagStripOffsets = new TiffTag( 273, 4, 1, 198 );
                tifftagSamplesPerPixel = new TiffTag( 277, 3, 1, 1 );
                tifftagRowsPerStrip = new TiffTag( 278, 4, 1, a_u32Height );
                tifftagStripByteCounts = new TiffTag( 279, 4, 1, a_u32Size );
                tifftagXResolution = new TiffTag( 282, 5, 1, 182 );
                tifftagYResolution = new TiffTag( 283, 5, 1, 190 );
                tifftagResolutionUnit = new TiffTag( 296, 3, 1, 2 );

                // Footer...
                u32NextIFD = 0;
                u64XResolution = ( ulong )0x100000000 + ( ulong )a_u32Resolution;
                u64YResolution = ( ulong )0x100000000 + ( ulong )a_u32Resolution;
            }

            // Header...
            public ushort u16ByteOrder;
            public ushort u16Version;
            public uint u32OffsetFirstIFD;

            // First IFD...
            public ushort u16IFD;

            // Tags...
            public TiffTag tifftagNewSubFileType;
            public TiffTag tifftagSubFileType;
            public TiffTag tifftagImageWidth;
            public TiffTag tifftagImageLength;
            public TiffTag tifftagBitsPerSample;
            public TiffTag tifftagCompression;
            public TiffTag tifftagPhotometricInterpretation;
            public TiffTag tifftagStripOffsets;
            public TiffTag tifftagSamplesPerPixel;
            public TiffTag tifftagRowsPerStrip;
            public TiffTag tifftagStripByteCounts;
            public TiffTag tifftagXResolution;
            public TiffTag tifftagYResolution;
            public TiffTag tifftagResolutionUnit;

            // Footer...
            public uint u32NextIFD;
            public ulong u64XResolution;
            public ulong u64YResolution;
        }
    }
}
