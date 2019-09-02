using System;
using System.Runtime.InteropServices;

namespace TwainDotNet.TwainNative
{
    internal static class Twain32Native
    {
        /*
         * Twain t/m versie 1.9
         * twain_32.dll werkt alleen als 32bits applicatie.
         * 
         * Twain vanaf versie 2.0
         * twaindsm.dll bestaat in 2 varianten: x86 en x64.
         * twaindsm laat geen WIA drivers zien; deze zijn wel te zien via twain_32.dll.
         * x86 ondersteund alleen 32 bits scanner drivers (dit zijn de meeste).
         * x64 ondersteund alleen 64 bits scanner drivers.
         */

        const string library = "twain_32.dll";
        //const string library = "TWAINDSM.DLL";

        /// <summary>
        /// DSM_Entry with a window handle as the parent parameter.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="zeroPtr">Should always be set to null.</param>
        /// <param name="dg"></param>
        /// <param name="dat"></param>
        /// <param name="msg"></param>
        /// <param name="windowHandle">The window handle that will act as the source's parent.</param>
        /// <returns></returns>
        [DllImport( library, EntryPoint = "#1")]
        public static extern TwainResult DsmParent([In, Out] Identity origin, IntPtr zeroPtr, DataGroup dg, DataArgumentType dat, Message msg, ref IntPtr windowHandle);

        /// <summary>
        /// DSM_Entry with an identity as the parameter
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="zeroPtr">Should always be set to null.</param>
        /// <param name="dg"></param>
        /// <param name="dat"></param>
        /// <param name="msg"></param>
        /// <param name="idds">The identity structure.</param>
        /// <returns></returns>
        [DllImport( library, EntryPoint = "#1")]
        public static extern TwainResult DsmIdentity([In, Out] Identity origin, IntPtr zeroPtr, DataGroup dg, DataArgumentType dat, Message msg, [In, Out] Identity idds);

        /// <summary>
        /// DSM_Entry with a user interface parameter. Acts on the data source.
        /// </summary>
        [DllImport( library, EntryPoint = "#1")]
        public static extern TwainResult DsUserInterface([In, Out] Identity origin, [In, Out] Identity dest, DataGroup dg, DataArgumentType dat, Message msg, UserInterface ui);

        [DllImport( library, EntryPoint = "#1")]
        public static extern TwainResult DsEvent([In, Out] Identity origin, [In, Out] Identity dest, DataGroup dg, DataArgumentType dat, Message msg, ref Event evt);

        [DllImport( library, EntryPoint = "#1")]
        public static extern TwainResult DsImageInfo([In, Out] Identity origin, [In] Identity dest, DataGroup dg, DataArgumentType dat, Message msg, [In, Out] ImageInfo imginf);

        [DllImport( library, EntryPoint = "#1")]
        public static extern TwainResult DsImageLayout([In, Out] Identity origin, [In, Out] Identity dest, DataGroup dg, DataArgumentType dat, Message msg, [In, Out] ImageLayout imglyt);

        [DllImport( library, EntryPoint = "#1")]
        public static extern TwainResult DsImageTransfer([In, Out] Identity origin, [In] Identity dest, DataGroup dg, DataArgumentType dat, Message msg, ref IntPtr hbitmap);

        [DllImport( library, EntryPoint = "#1" )]
        public static extern TwainResult DsSetupMemXfer( [In, Out] Identity origin, [In] Identity dest, DataGroup dg, DataArgumentType dat, Message msg, [In, Out] SetupMemXfer memxfer );

        [DllImport( library, EntryPoint = "#1" )]
        public static extern TwainResult DsImageMemXfer( [In, Out] Identity origin, [In] Identity dest, DataGroup dg, DataArgumentType dat, Message msg, [In, Out] ImageMemXfer memxfer );

        [DllImport( library, EntryPoint = "#1")]
        public static extern TwainResult DsPendingTransfer([In, Out] Identity origin, [In] Identity dest, DataGroup dg, DataArgumentType dat, Message msg, [In, Out] PendingXfers pxfr);

        [DllImport( library, EntryPoint = "#1")]
        public static extern TwainResult DsStatus([In, Out] Identity origin, [In] Identity dest, DataGroup dg, DataArgumentType dat, Message msg, [In, Out] Status dsmstat);

        [DllImport( library, EntryPoint = "#1")]
        public static extern TwainResult DsmStatus([In, Out] Identity origin, [In] Identity dest, DataGroup dg, DataArgumentType dat, Message msg, [In, Out] Status dsmstat);

        [DllImport( library, EntryPoint = "#1")]
        public static extern TwainResult DsCapability([In, Out] Identity origin, [In] Identity dest, DataGroup dg, DataArgumentType dat, Message msg, [In, Out] TwainCapability capa);

    }
}
