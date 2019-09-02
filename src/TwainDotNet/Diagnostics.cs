using System;
using System.Collections.Generic;
using System.Text;
using TwainDotNet.TwainNative;

namespace TwainDotNet
{
	/// <summary>
	/// Diagnostict.
	/// </summary>
    public class Diagnostics
    {
		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="messageHook">The message hook.</param>
        public Diagnostics(IWindowsMessageHook messageHook)
        {
            using (var dataSourceManager = new DataSourceManager(DataSourceManager.DefaultApplicationId, messageHook))
            {
                dataSourceManager.SelectSource();

                var dataSource = dataSourceManager.DataSource;
                dataSource.OpenSource();

                foreach (Capabilities capability in Enum.GetValues(typeof(Capabilities)))
                {
                    try
                    {
                        var result = Capability.GetBoolCapability(capability, dataSourceManager.ApplicationId, dataSource.SourceId);

                        Console.WriteLine("{0}: {1}", capability, result);
                    }
                    catch (TwainException e)
                    {
                        Console.WriteLine("{0}: {1} {2} {3}", capability, e.Message, e.ReturnCode, e.ConditionCode);
                    }
                }
            }
        }
    }
}
