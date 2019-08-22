using Blazor.Fluxor;
using FlightFinder.Shared;

namespace FlightFinder.Client.Store
{
	public class FetchAirportsCompleteAction 
	{
		public readonly Airport[] Airports;

		public FetchAirportsCompleteAction(Airport[] airports)
		{
			Airports = airports;
		}
	}
}
