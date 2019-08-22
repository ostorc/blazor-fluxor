using Blazor.Fluxor;
using WeatherForecastSample.Shared;

namespace WeatherForecastSample.Client.Store.FetchData
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
