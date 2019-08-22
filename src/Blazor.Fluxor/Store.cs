using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Blazor.Fluxor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Blazor.Fluxor
{
	/// <see cref="IStore"/>
	public class Store : IStore
	{
		/// <see cref="IStore.Features"/>
		public IReadOnlyDictionary<string, IFeature> Features => FeaturesByName;
		/// <see cref="IStore.Initialized"/>
		public Task Initialized => InitializedCompletionSource.Task;

		private IBrowserInteropService BrowserInteropService;
		private readonly Dictionary<string, IFeature> FeaturesByName = new Dictionary<string, IFeature>(StringComparer.InvariantCultureIgnoreCase);
		private readonly List<IEffect> Effects = new List<IEffect>();
		private readonly List<IMiddleware> Middlewares = new List<IMiddleware>();
		private readonly List<IMiddleware> ReversedMiddlewares = new List<IMiddleware>();
		private readonly Queue<object> QueuedActions = new Queue<object>();
		private readonly TaskCompletionSource<bool> InitializedCompletionSource = new TaskCompletionSource<bool>();

		private int BeginMiddlewareChangeCount;
		private bool HasActivatedStore;
		private bool IsInsideMiddlewareChange => BeginMiddlewareChangeCount > 0;

		/// <summary>
		/// Creates an instance of the store
		/// </summary>
		/// <param name="browserInteropService">The BrowserInteropService the Browser will use to initialise the store</param>
		public Store(IBrowserInteropService browserInteropService)
		{
			BrowserInteropService = browserInteropService;
			BrowserInteropService.PageLoaded += OnPageLoaded;
			Dispatch(new StoreInitializedAction());
		}

		/// <see cref="IStore.AddFeature(IFeature)"/>
		public void AddFeature(IFeature feature)
		{
			if (feature == null)
				throw new ArgumentNullException(nameof(feature));

			FeaturesByName.Add(feature.GetName(), feature);
		}

		/// <see cref="IDispatcher.Dispatch(object)"/>
		public void Dispatch(object action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			// Do not allow task dispatching inside a middleware-change.
			// These change cycles are for things like "jump to state" in Redux Dev Tools
			// and should be short lived.
			// We avoid dispatching inside a middleware change because we don't want UI events (like component Init)
			// that trigger actions (such as fetching data from a server) to execute
			if (IsInsideMiddlewareChange)
				return;

			// If there was already an action in the Queue then an action dispatch is already in progress, so we will just
			// let this new action be added to the queue and then exit
			// Note: This is to cater for the following scenario
			//	1: An action is dispatched
			//	2: An effect is triggered
			//	3: The effect immediately dispatches a new action
			// The Queue ensures it is processed after its triggering action has completed rather than immediately
			bool wasAlreadyDispatching = QueuedActions.Any();
			QueuedActions.Enqueue(action);
			if (wasAlreadyDispatching)
				return;

			// HasActivatedStore is set to true when the page finishes loading
			// At which point DequeueActions will be called
			if (!HasActivatedStore)
				return;

			DequeueActions();
		}

		/// <see cref="IStore.AddEffect(IEffect)"/>
		public void AddEffect(IEffect effect)
		{
			if (effect == null)
				throw new ArgumentNullException(nameof(effect));
			Effects.Add(effect);
		}

		/// <see cref="IStore.AddMiddleware(IMiddleware)"/>
		public void AddMiddleware(IMiddleware middleware)
		{
			Middlewares.Add(middleware);
			ReversedMiddlewares.Insert(0, middleware);
			// Initialize the middleware immediately if the store has already been initialized, otherwise this will be
			// done the first time Dispatch is called
			if (HasActivatedStore)
			{
				middleware.Initialize(this);
				middleware.AfterInitializeAllMiddlewares();
			}
		}

		/// <see cref="IStore.BeginInternalMiddlewareChange"/>
		public IDisposable BeginInternalMiddlewareChange()
		{
			BeginMiddlewareChangeCount++;
			IDisposable[] disposables = Middlewares
				.Select(x => x.BeginInternalMiddlewareChange())
				.ToArray();
			return new DisposableCallback(() =>
			{
				BeginMiddlewareChangeCount--;
				if (BeginMiddlewareChangeCount == 0)
					disposables.ToList().ForEach(x => x.Dispose());
			});
		}

		/// <see cref="IStore.Initialize"/>
		public RenderFragment Initialize()
		{
			return (RenderTreeBuilder renderer) =>
			{
				int sequence = 0;
				foreach (IMiddleware middleware in Middlewares)
				{
					sequence++;
					string middlewareScript = middleware.GetClientScripts();
					if (middlewareScript != null)
					{
						renderer.OpenElement(sequence, "script");
						renderer.AddMarkupContent(sequence, $"// Middleware scripts: {middleware.GetType().FullName}\r\n{middlewareScript}");
						renderer.CloseElement();
					}
				}

				renderer.OpenElement(sequence++, "script");
				renderer.AddMarkupContent(sequence, GetClientScripts());
				renderer.CloseElement();
			};
		}

		private void TriggerEffects(object action)
		{
			var effectsToTrigger = Effects.Where(x => x.ShouldReactToAction(action));
			foreach (var effect in effectsToTrigger)
				effect.HandleAsync(action, this);
		}

		private void InitializeMiddlewares()
		{
			Middlewares.ForEach(x => x.Initialize(this));
			Middlewares.ForEach(x => x.AfterInitializeAllMiddlewares());
		}

		private void ExecuteMiddlewareBeforeDispatch(object actionAboutToBeDispatched)
		{
			foreach (IMiddleware middleWare in Middlewares)
				middleWare.BeforeDispatch(actionAboutToBeDispatched);
		}

		private void ExecuteMiddlewareAfterDispatch(object actionJustDispatched)
		{
			Middlewares.ForEach(x => x.AfterDispatch(actionJustDispatched));
		}

		private void NotifyFeatureOfDispatch(IFeature feature, object action)
		{
			string methodName = nameof(IFeature.ReceiveDispatchNotificationFromStore);
			// We need the generic method for the feature instance
			MethodInfo methodInfo = feature
				.GetType()
				.GetMethod(methodName);

			methodInfo.Invoke(feature, new object[] { action });
		}

		private void ActivateStore()
		{
			if (HasActivatedStore)
				return;

			HasActivatedStore = true;
			InitializeMiddlewares();
			DequeueActions();
			InitializedCompletionSource.SetResult(true);
		}

		private void DequeueActions()
		{
			while (QueuedActions.Any())
			{
				// We want the next action but we won't dequeue it because we use
				// a non-empty queue as an indication that a Dispatch() loop is already in progress
				object nextActionToDequeue = QueuedActions.Peek();
				// Only process the action if no middleware vetos it
				if (Middlewares.All(x => x.MayDispatchAction(nextActionToDequeue)))
				{
					ExecuteMiddlewareBeforeDispatch(nextActionToDequeue);

					// Notify all features of this action
					foreach (var featureInstance in FeaturesByName.Values)
					{
						NotifyFeatureOfDispatch(featureInstance, nextActionToDequeue);
					};

					ExecuteMiddlewareAfterDispatch(nextActionToDequeue);

					TriggerEffects(nextActionToDequeue);
				}
				// Now remove the processed action from the queue so we can move on to the next (if any)
				QueuedActions.Dequeue();
			}
		}

		private string GetClientScripts()
		{
			return BrowserInteropService.GetClientScripts();
		}

		private void OnPageLoaded(object sender, EventArgs e)
		{
			BrowserInteropService.PageLoaded -= OnPageLoaded;
			ActivateStore();
		}
	}
}
