using Blazor.Fluxor;
using FlightFinder.Shared;

namespace FlightFinder.Client.Store
{
	public class RemoveFromShortlistAction 
	{
		public readonly Itinerary Itinerary;

		public RemoveFromShortlistAction(Itinerary itinerary)
		{
			Itinerary = itinerary;
		}
	}
}
