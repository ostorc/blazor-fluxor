using System;

namespace ReduxDevToolsIntegration.Client.Store.Counter
{
	public class CounterState
	{
		//TODO: Private setter when JSON supports it
		public int ClickCount { get; set; }

		[Obsolete("Used for deserialization only")]
		public CounterState() { }

		public CounterState(int clickCount)
		{
			ClickCount = clickCount;
		}
	}
}