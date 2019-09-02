using System;
using TwainDotNet.TwainNative;
using log4net;

namespace TwainDotNet
{
    internal class Capability
    {
        /// <summary>
        /// The logger for this class.
        /// </summary>
        static readonly ILog log = LogManager.GetLogger( typeof( Capability ) );

        readonly Identity _applicationId;
        readonly Identity _sourceId;
        readonly Capabilities _capability;

        public Capability( Capabilities capability, Identity applicationId, Identity sourceId )
        {
            _capability = capability;
            _applicationId = applicationId;
            _sourceId = sourceId;
        }

        public BasicCapabilityResult GetBasicValue()
        {
            CapabilityResult result = GetValueInternal( Message.Get );
            if( result is BasicCapabilityResult basicCapabilityResult1 )
            {
                return basicCapabilityResult1;
            }

            if( result is EnumCapabilityResult )
            {
                result = GetValueInternal( Message.GetCurrent );
                if( result is BasicCapabilityResult basicCapabilityResult2 )
                {
                    return basicCapabilityResult2;
                }
            }

			if( result is RangeCapabilityResult )
			{
				result = GetValueInternal( Message.GetCurrent );
				if( result is BasicCapabilityResult basicCapabilityResult2 )
				{
					return basicCapabilityResult2;
				}
			}

			throw new InvalidOperationException( "Unsupported basic value." );
        }

        public CapabilityResult GetValue()
        {
            return GetValueInternal( Message.Get );
        }

        private CapabilityResult GetValueInternal( Message message )
        {
            using( TwainCapability twainCapability = new TwainCapability( _capability ) )
            {
                var result = Twain32Native.DsCapability(
                        _applicationId,
                        _sourceId,
                        DataGroup.Control,
                        DataArgumentType.Capability,
                        message,
                        twainCapability );

                if( result != TwainResult.Success )
                {
                    var conditionCode = GetStatus();

                    log.Debug( string.Format( "Failed to get capability:{0} reason: {1}",
                        _capability, conditionCode ) );

                    throw new TwainException( string.Format( "Unsupported capability {0} ({1}).", _capability, message ), result, conditionCode );
                }

                return twainCapability.ReadValue();
            }
        }

        protected ConditionCode GetStatus()
        {
            return DataSourceManager.GetConditionCode( _applicationId, _sourceId );
        }

        protected void SetValue( TwainType twainType, int rawValue )
        {
            log.Debug( string.Format( "Attempting to set capabilities:{0}, value:{1}, type:{1}",
                _capability, rawValue, twainType ) );

            var oneValue = new CapabilityOneValue( twainType, rawValue );
            TwainResult result;
            using( var twainCapability = new TwainCapability( _capability ) )
            {
                twainCapability.WriteValue( oneValue );
                result = Twain32Native.DsCapability(
                        _applicationId,
                        _sourceId,
                        DataGroup.Control,
                        DataArgumentType.Capability,
                        Message.Set,
                        twainCapability );
            }

            if( result != TwainResult.Success )
            {
                log.Debug( string.Format( "Failed to set capabilities:{0}, value:{1}, type:{1}, result:{2}",
                    _capability, rawValue, twainType, result ) );

                if( result == TwainResult.Failure )
                {
                    var conditionCode = GetStatus();

                    log.Error( string.Format( "Failed to set capabilites:{0} reason: {1}",
                        _capability, conditionCode ) );

                    throw new TwainException( "Failed to set capability.", result, conditionCode );
                }
                else if( result == TwainResult.CheckStatus )
                {
                    log.Debug( "Value changed but not to requested value" );
                }
                else
                {
                    throw new TwainException( "Failed to set capability.", result );
                }
            }
            else
            {
                log.Debug( "Set capabilities successfully" );
            }
        }

        public static int SetCapability( Capabilities capability, int rawValue, TwainType twainType, Identity applicationId,
            Identity sourceId )
        {
            var c = new Capability( capability, applicationId, sourceId );
            var capResult = c.GetBasicValue();

            if( twainType != capResult.TwainType )
            {
                throw new TwainException( string.Format( "Capability {0} TwainType mismatch. Expected: {1}; Actual: {2}", capability, twainType, capResult.TwainType ) );
            }

            if( capResult.RawBasicValue == rawValue )
            {
                // Value is already set
                return rawValue;
            }

            // TODO: Check the set of Available Values that are supported by the Source for that
            // capability.

            //if (value in set of available values)
            //{
            c.SetValue( twainType, rawValue );
            //}

            // Verify that the new values have been accepted by the Source.
            capResult = c.GetBasicValue();

            // Check that the device supports the capability
            if( capResult.RawBasicValue != rawValue )
            {
                log.Info( string.Format( "Unable to set specified value for capability {0}. Current value={1}; Requested value={2}", capability, capResult.RawBasicValue, rawValue ) );
            }

            return capResult.RawBasicValue;
        }

        public static void SetCapability( Capabilities capability, bool value, Identity applicationId,
            Identity sourceId )
        {
            SetCapability( capability, value ? 1 : 0, TwainType.Bool, applicationId, sourceId );
        }

        public static void SetCapability( Capabilities capability, Fix32 fix32, Identity applicationId,
            Identity sourceId )
        {
            byte[] rawBytes = new byte[4];
            byte[] bWhole = BitConverter.GetBytes( fix32.Whole );
            byte[] bFrac = BitConverter.GetBytes( fix32.Frac );
            Array.Copy( bWhole, rawBytes, 2 );
            Array.Copy( bFrac, 0, rawBytes, 2, 2 );
            SetCapability( capability, BitConverter.ToInt32( rawBytes, 0 ), TwainType.Fix32, applicationId, sourceId );
        }

        public static bool GetBoolCapability( Capabilities capability, Identity applicationId,
            Identity sourceId )
        {
            var c = new Capability( capability, applicationId, sourceId );
            var capResult = c.GetBasicValue();

            return capResult.BoolValue;
        }

        public static CapabilityResult GetCapability( Capabilities capability, Identity applicationId,
            Identity sourceId )
        {
			return GetCapability( capability, Message.Get, applicationId, sourceId );
        }

		public static CapabilityResult GetCapability( Capabilities capability, Message message, Identity applicationId,
		Identity sourceId )
		{
			var c = new Capability( capability, applicationId, sourceId );
			return c.GetValueInternal( message );
		}
	}
}
