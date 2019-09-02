using System;

namespace TwainDotNet
{
	/// <summary>
	/// Transfer image event args.
	/// </summary>
    public class TransferImageEventArgs : EventArgs
    {
		/// <summary>
		/// Gets the hBitmap.
		/// </summary>
        public IntPtr HBitmap
        {
            get; private set;
        }

		/// <summary>
		/// Gets the memory transfer data.
		/// </summary>
        public MemoryTransferData MemoryTransferData
        {
            get; private set;
        }

		/// <summary>
		/// Whether to continue scanning.
		/// </summary>
        public bool ContinueScanning
        {
            get; set;
        }

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="hbitmap">The hBitmap.</param>
		/// <param name="continueScanning">Whether to continue scanning.</param>
        public TransferImageEventArgs( IntPtr hbitmap, bool continueScanning )
        {
            HBitmap = hbitmap;
            ContinueScanning = continueScanning;
        }

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="memoryTransferData">The memory transfer data.</param>
		/// <param name="continueScanning">Whether to continue scanning.</param>
        public TransferImageEventArgs( MemoryTransferData memoryTransferData, bool continueScanning )
        {
            MemoryTransferData = memoryTransferData;
            ContinueScanning = continueScanning;
        }
    }
}
