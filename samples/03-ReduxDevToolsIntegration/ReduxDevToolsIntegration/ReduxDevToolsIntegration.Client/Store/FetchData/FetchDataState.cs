using ReduxDevToolsIntegration.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReduxDevToolsIntegration.Client.Store.FetchData
{
	public class FetchDataState
	{
		//TODO: Private setters when JSON allows it
		public bool IsLoading { get; set; }
		public string ErrorMessage { get; set; }
		public WeatherForecast[] Forecasts { get; set; }

		[Obsolete("Used for deserialization only")]
		public FetchDataState() { }

		public FetchDataState(bool isLoading, string errorMessage, IEnumerable<WeatherForecast> forecasts)
		{
			IsLoading = isLoading;
			ErrorMessage = errorMessage;
			Forecasts = forecasts == null ? null : forecasts.ToArray();
		}
	}
}