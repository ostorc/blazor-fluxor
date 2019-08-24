using Blazor.Fluxor.DependencyInjection.DependencyScanners;
using Blazor.Fluxor.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Blazor.Fluxor.DependencyInjection
{
	internal static class DependencyScanner
	{
		internal static void Scan(this IServiceCollection serviceCollection,
			IEnumerable<AssemblyScanSettings> assembliesToScan, IEnumerable<AssemblyScanSettings> scanWhitelist)
		{
			if (assembliesToScan == null || assembliesToScan.Count() == 0)
				throw new ArgumentNullException(nameof(assembliesToScan));
			scanWhitelist = scanWhitelist ?? new List<AssemblyScanSettings>();

			IEnumerable<Type> allCandidateTypes = assembliesToScan.SelectMany(x => x.Assembly.GetTypes())
				.Union(scanWhitelist.SelectMany(x => x.Assembly.GetTypes()))
				.Where(t => !t.IsAbstract)
				.Distinct();
			IEnumerable<Assembly> allCandidateAssemblies = assembliesToScan.Select(x => x.Assembly)
				.Union(scanWhitelist.Select(x => x.Assembly))
				.Distinct();

			IEnumerable<AssemblyScanSettings> scanBlacklist =
				MiddlewareScanner.FindMiddlewareLocations(allCandidateAssemblies);
			allCandidateTypes = AssemblyScanSettings.Filter(
				types: allCandidateTypes,
				scanBlacklist: scanBlacklist,
				scanWhitelist: scanWhitelist);


			IEnumerable<DiscoveredReducerInfo> discoveredReducerInfos =
				ReducersRegistration.DiscoverReducers(serviceCollection, allCandidateTypes);

			IEnumerable<DiscoveredEffectInfo> discoveredEffectInfos =
				EffectsRegistration.DiscoverEffects(serviceCollection, allCandidateTypes);

			IEnumerable<DiscoveredFeatureInfo> discoveredFeatureInfos =
				FeaturesRegistration.DiscoverFeatures(serviceCollection, allCandidateTypes, discoveredReducerInfos);

			RegisterStore(serviceCollection, discoveredFeatureInfos, discoveredEffectInfos);
		}

		private static void RegisterStore(IServiceCollection serviceCollection, 
			IEnumerable<DiscoveredFeatureInfo> discoveredFeatureInfos,
			IEnumerable<DiscoveredEffectInfo> discoveredEffectInfos)
		{
			// Register IDispatcher as an alias to IStore
			serviceCollection.AddScoped<IDispatcher>(sp => sp.GetService<IStore>());

			// Register a custom factory for building IStore that will inject all effects
			serviceCollection.AddScoped(typeof(IStore), serviceProvider =>
			{
				var browserInteropService = serviceProvider.GetService<IBrowserInteropService>();
				var store = new Store(browserInteropService);
				foreach(DiscoveredFeatureInfo discoveredFeatureInfo in discoveredFeatureInfos)
				{
					IFeature feature = (IFeature)serviceProvider.GetService(discoveredFeatureInfo.FeatureInterfaceGenericType);
					store.AddFeature(feature);
				}

				foreach(DiscoveredEffectInfo discoveredEffectInfo in discoveredEffectInfos)
				{
					IEffect effect = (IEffect)serviceProvider.GetService(discoveredEffectInfo.ImplementingType);
					store.AddEffect(effect);
				}

				foreach (Type middlewareType in Options.MiddlewareTypes)
				{
					IMiddleware middleware = (IMiddleware)serviceProvider.GetService(middlewareType);
					store.AddMiddleware(middleware);
				}

				return store;
			});
		}
	}
}
