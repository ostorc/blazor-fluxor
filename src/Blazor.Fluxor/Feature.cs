using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Blazor.Fluxor
{
	/// <see cref="IFeature{TState}"/>
	public abstract class Feature<TState> : IFeature<TState>
	{
		/// <see cref="IFeature.GetName"/>
		public abstract string GetName();

		/// <see cref="IFeature.GetState"/>
		public virtual object GetState() => State;

		/// <see cref="IFeature.RestoreState(object)"/>
		public virtual void RestoreState(object value) => State = (TState)value;

		/// <see cref="IFeature.GetStateType"/>
		public virtual Type GetStateType() => typeof(TState);

		/// <summary>
		/// Gets the initial state for the feature
		/// </summary>
		/// <returns>The initial state</returns>
		protected abstract TState GetInitialState();

		/// <summary>
		/// A list of reducers registered with this feature
		/// </summary>
		protected readonly List<IReducer<TState>> Reducers = new List<IReducer<TState>>();

		private MethodInfo BlazorComponentStateHasChangedMethod;
		private List<WeakReference<ComponentBase>> ObservingComponents = new List<WeakReference<ComponentBase>>();

		/// <summary>
		/// Creates a new instance
		/// </summary>
		public Feature()
		{
			State = GetInitialState();
			BlazorComponentStateHasChangedMethod = typeof(ComponentBase).GetMethod("StateHasChanged", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		private TState _State;

		/// <summary>
		/// Event that is executed whenever the state changes
		/// </summary>
		public event EventHandler<TState> StateChanged;

		/// <see cref="IFeature{TState}.State"/>
		public virtual TState State
		{
			get => _State;
			protected set
			{
				bool stateHasChanged = !Object.ReferenceEquals(_State, value);
				_State = value;
				if (stateHasChanged)
					TriggerStateChangedCallbacks(value);
			}
		}

		/// <see cref="IFeature{TState}.AddReducer(IReducer{TState})"/>
		public virtual void AddReducer(IReducer<TState> reducer)
		{
			if (reducer == null)
				throw new ArgumentNullException(nameof(reducer));
			Reducers.Add(reducer);
		}

		/// <see cref="IFeature.ReceiveDispatchNotificationFromStore(object)"/>
		public virtual void ReceiveDispatchNotificationFromStore(object action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			IEnumerable<IReducer<TState>> applicableReducers = Reducers.Where(x => x.ShouldReduceStateForAction(action));
			TState newState = State;
			foreach (IReducer<TState> currentReducer in applicableReducers)
			{
				newState = currentReducer.Reduce(newState, action);
			}
			State = newState;
		}

		/// <see cref="IFeature.Subscribe(ComponentBase)"/>
		public void Subscribe(ComponentBase subscriber)
		{
			var subscriberReference = new WeakReference<ComponentBase>(subscriber);
			ObservingComponents.Add(subscriberReference);
		}

		private void TriggerStateChangedCallbacks(TState newState)
		{
			var subscribers = new List<ComponentBase>();
			var callbacks = new List<Action>();
			var newStateChangedCallbacks = new List<WeakReference<ComponentBase>>();

			// Keep only weak references that have not expired
			foreach (var subscription in ObservingComponents)
			{
				subscription.TryGetTarget(out ComponentBase subscriber);
				if (subscriber != null)
				{
					// Keep a reference to the subscribers stop them being collected before we have finished
					subscribers.Add(subscriber);
					// Create a callback
					callbacks.Add(() => BlazorComponentStateHasChangedMethod.Invoke(subscriber, null));
					// Add this observer to the replacement list
					newStateChangedCallbacks.Add(subscription);
				}
			}

			ObservingComponents = newStateChangedCallbacks;

			// Execute the callbacks
			callbacks.ForEach(callback => callback());

			// Keep observers and callbacks alive until after we have called them
			GC.KeepAlive(subscribers);
			GC.KeepAlive(callbacks);

			StateChanged?.Invoke(this, newState);
		}
	}
}
