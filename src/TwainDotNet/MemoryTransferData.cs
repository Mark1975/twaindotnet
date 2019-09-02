using TwainDotNet.TwainNative;

namespace TwainDotNet
{
	/// <summary>
	/// Memory transfer data.
	/// </summary>
    public class MemoryTransferData
    {
		/// <summary>
		/// Gets or sets the data.
		/// </summary>
        public byte[] Data
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the units.
		/// </summary>
		public Units Units
		{
			get; set;
		}

		/// <summary>
		/// Gets or sets the image info.
		/// </summary>
        public ImageInfo ImageInfo
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the image layout.
		/// </summary>
		public ImageLayout ImageLayout
		{
			get; set;
		}

		/// <summary>
		/// Gets or sets the iamge mem xfer.
		/// </summary>
        public ImageMemXfer ImageMemXfer
        {
            get; set;
        }
    }
}
