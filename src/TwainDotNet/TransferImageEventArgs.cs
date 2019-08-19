using System;
using System.Drawing;

namespace TwainDotNet
{
    public class TransferImageEventArgs : EventArgs
    {
        public Bitmap Image
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

        public TransferImageEventArgs( Bitmap image, bool continueScanning )
        {
            Image = image;
            ContinueScanning = continueScanning;
        }
        public TransferImageEventArgs( MemoryTransferData memoryTransferData, bool continueScanning )
        {
            MemoryTransferData = memoryTransferData;
            ContinueScanning = continueScanning;
        }
    }
}
