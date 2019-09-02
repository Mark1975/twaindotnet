using System.Runtime.InteropServices;

namespace TwainDotNet.TwainNative
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class Status
    {
        public ConditionCode ConditionCode;
        public short Reserved;
    }
}
