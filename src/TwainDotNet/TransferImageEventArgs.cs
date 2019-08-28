using System;
using System.Drawing;

namespace TwainDotNet
{
    public class TransferImageEventArgs : EventArgs
    {
        public IntPtr HBitmap
        {
            get; private set;
        }
        public MemoryTransferData MemoryTransferData
        {
            get; private set;
        }
        public bool ContinueScanning
        {
            get; set;
        }

        public TransferImageEventArgs( IntPtr hbitmap, bool continueScanning )
        {
            HBitmap = hbitmap;
            ContinueScanning = continueScanning;
        }
        public TransferImageEventArgs( MemoryTransferData memoryTransferData, bool continueScanning )
        {
            MemoryTransferData = memoryTransferData;
            ContinueScanning = continueScanning;
        }
    }
}
