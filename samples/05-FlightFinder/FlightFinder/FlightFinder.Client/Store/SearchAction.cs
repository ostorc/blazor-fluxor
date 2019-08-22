using Blazor.Fluxor;
using FlightFinder.Shared;

namespace FlightFinder.Client.Store
{
	public class SearchAction
	{
		public readonly SearchCriteria SearchCriteria;

		public SearchAction(SearchCriteria searchCriteria)
		{
			SearchCriteria = searchCriteria;
		}
	}
}
