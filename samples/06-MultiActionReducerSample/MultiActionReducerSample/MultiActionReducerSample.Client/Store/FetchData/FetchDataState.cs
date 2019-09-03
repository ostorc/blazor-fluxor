using MultiActionReducerSample.Shared;

namespace MultiActionReducerSample.Client.Store.FetchData
{
	public class FetchDataState
	{
		public bool IsLoading { get; private set; }
		public string ErrorMessage { get; private set; }
		public WeatherForecast[] Forecasts { get; private set; }

		public FetchDataState(bool isLoading, string errorMessage, WeatherForecast[] forecasts)
		{
			IsLoading = isLoading;
			ErrorMessage = errorMessage;
			Forecasts = forecasts;
		}
	}
}
