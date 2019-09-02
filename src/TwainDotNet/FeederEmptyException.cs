using System;
using System.Runtime.Serialization;

namespace TwainDotNet
{
    internal class FeederEmptyException : TwainException
    {
        public FeederEmptyException()
            : this(null, null)
        {
        }

        public FeederEmptyException(string message)
            : this(message, null)
        {
        }

        protected FeederEmptyException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }

        public FeederEmptyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
