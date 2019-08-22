namespace Blazor.Fluxor
{
	/// <summary>
	/// A generic implementation of a reducer
	/// </summary>
	/// <typeparam name="TState">The state that this reducer works with</typeparam>
	/// <typeparam name="TAction">The action type this reducer responds to</typeparam>
	public abstract class Reducer<TState, TAction> : IReducer<TState>
	{
		/// <summary>
		/// <see cref="IReducer{TState}.ShouldReduceStateForAction(object)"/>
		/// </summary>
		public bool ShouldReduceStateForAction(object action) => action is TAction;

		/// <summary>
		/// Reduces state in reaction to the action dispatched via the store.
		/// </summary>
		/// <param name="state">The state type this reducer handles</param>
		/// <param name="action">The action type this reducer handles</param>
		/// <returns>The new state</returns>
		public abstract TState Reduce(TState state, TAction action);

		TState IReducer<TState>.Reduce(TState state, object action) => Reduce(state, (TAction)action);
	}
}
