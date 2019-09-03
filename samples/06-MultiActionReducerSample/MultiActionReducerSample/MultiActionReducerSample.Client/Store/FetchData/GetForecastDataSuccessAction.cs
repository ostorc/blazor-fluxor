using Blazor.Fluxor;
using MultiActionReducerSample.Shared;

namespace MultiActionReducerSample.Client.Store.FetchData
{
	public class GetForecastDataSuccessAction 
	{
		public WeatherForecast[] WeatherForecasts { get; private set; }

		public GetForecastDataSuccessAction(WeatherForecast[] weatherForecasts)
		{
			WeatherForecasts = weatherForecasts;
		}
	}
}
