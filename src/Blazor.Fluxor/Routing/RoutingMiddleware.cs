using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Blazor.Fluxor.Routing
{
	/// <summary>
	/// Adds support for routing <see cref="Microsoft.AspNetCore.Components.IUriHelper"/>
	/// via a Fluxor store.
	/// </summary>
	public class RoutingMiddleware : Middleware
	{
		private readonly IUriHelper UriHelper;
		private readonly IFeature<RoutingState> Feature;

		/// <summary>
		/// Creates a new instance of the routing middleware
		/// </summary>
		/// <param name="uriHelper">Uri helper</param>
		/// <param name="feature">The routing feature</param>
		public RoutingMiddleware(IUriHelper uriHelper, IFeature<RoutingState> feature)
		{
			UriHelper = uriHelper;
			Feature = feature;
			UriHelper.OnLocationChanged += OnLocationChanged;
		}


		/// <see cref="IMiddleware.Initialize(IStore)"/>
		public override void Initialize(IStore store)
		{
			base.Initialize(store);
			// If the URL changed before we initialized then dispatch an action
			Store.Dispatch(new Go(UriHelper.GetAbsoluteUri()));
		}

		/// <see cref="Middleware.OnInternalMiddlewareChangeEnding"/>
		protected override void OnInternalMiddlewareChangeEnding()
		{
			if (Feature.State.Uri != UriHelper.GetAbsoluteUri())
				UriHelper.NavigateTo(Feature.State.Uri);
		}

		private void OnLocationChanged(object sender, LocationChangedEventArgs e)
		{
			string fullUri = UriHelper.ToAbsoluteUri(e.Location).ToString();
			if (Store != null && !IsInsideMiddlewareChange && fullUri != Feature.State.Uri)
				Store.Dispatch(new Go(e.Location));
		}
	}
}
