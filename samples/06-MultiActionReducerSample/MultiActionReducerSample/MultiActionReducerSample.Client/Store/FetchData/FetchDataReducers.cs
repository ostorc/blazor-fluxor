using Blazor.Fluxor;

namespace MultiActionReducerSample.Client.Store.FetchData
{
	public class FetchDataReducers : MultiActionReducer<FetchDataState>
	{
		public FetchDataReducers()
		{
			AddActionReducer<GetForecastDataAction>((state, action) =>
				new FetchDataState(
				isLoading: true,
				errorMessage: null,
				forecasts: null));

			AddActionReducer<GetForecastDataFailedAction>((state, action) =>
				new FetchDataState(
					isLoading: false,
					errorMessage: action.ErrorMessage,
					forecasts: null));

			AddActionReducer<GetForecastDataSuccessAction>((state, action) =>
				new FetchDataState(
					isLoading: false,
					errorMessage: null,
					forecasts: action.WeatherForecasts));
		}
	}
}
