using Blazor.Fluxor;

namespace ReduxDevToolsIntegration.Client.Store.FetchData
{
	public class GetForecastDataFailedAction 
	{
		public string ErrorMessage { get; private set; }

		public GetForecastDataFailedAction(string errorMessage)
		{
			ErrorMessage = errorMessage;
		}
	}
}