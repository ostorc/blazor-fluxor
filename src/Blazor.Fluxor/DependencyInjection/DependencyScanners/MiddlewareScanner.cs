﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Blazor.Fluxor.DependencyInjection.DependencyScanners
{
	internal class MiddlewareScanner
	{
		internal static IEnumerable<AssemblyScanSettings> FindMiddlewareLocations(IEnumerable<Assembly> assembliesToScan)
		{
			return assembliesToScan
				.SelectMany(x => x.GetTypes().Where(t => t.GetInterfaces().Any(i => i == typeof(IMiddleware))))
				.Select(x => new AssemblyScanSettings(x.Assembly, x.Namespace))
				.Distinct();
		}
	}
}
