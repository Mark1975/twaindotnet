using TwainDotNet.TwainNative;

namespace TwainDotNet
{
    public class MemoryTransferData
    {
        public byte[] Data
        {
            get; set;
        }

		public Units Units
		{
			get; set;
		}

        public ImageInfo ImageInfo
        {
            get; set;
        }

		public ImageLayout ImageLayout
		{
			get; set;
		}

        public ImageMemXfer ImageMemXfer
        {
            get; set;
        }
    }
}
