# 01-CounterSample
This sample shows how to take the standard Visual Studio Blazor template and Fluxorize it.

### Creating the project
1. Create a new Blazor website using the template in Visual Studio (See the [Official Blazor-Fluxor nuget page] for details on how to install Blazor).
2. Name the project `CounterSample`.
3. Add the Blazor-Fluxor nuget package to your Client project.
 
### Initializing the store
Fluxor has the ability to write your own middleware libraries or use existing 3rd party libraries. As these libraries may require Javascript inserted into the hosting html you must first call `@Store.Initialize()` in your main layout page.
1. In the Client project open the file `Shared\MainLayout.razor`
2. Beneath the `@inherits LayoutComponentBase` line add `@inject Blazor.Fluxor.IStore Store`
3. Then add `@Store.Initialize()` - This will initialize the store and inject any required Javascript

### Automatic discovery of store features
1. In the Client project find the `Startup.cs` file. 
2. Add `using Blazor.Fluxor;`
3. Change the `ConfigureServices` method to add Fluxor
```c#
public void ConfigureServices(IServiceCollection services)
{
	services.AddFluxor(options => options
		.UseDependencyInjection(typeof(Startup).Assembly)
	);
});
```

### Adding state
1. In the Client project add a folder named `Store`.
2. It is recommended that you create a folder per feature of your application. Create a folder named `Counter`.
3. Within the `Counter` folder create a file named `CounterState.cs`.
3. Enter the following code. It is good practice to make the properties of state have private setters. This ensures state can only be modified by dispatching an action through the store rather than allowing it to be edited in-place.
```c#
namespace CounterSample.Client.Store.Counter
{
	public class CounterState
	{
		public int ClickCount { get; private set; }

		public CounterState(int clickCount)
		{
			ClickCount = clickCount;
		}
	}
}
```
4. Now that we have state for the Counter feature we need to create the feature itself. Create a file named `CounterFeature.cs` and enter the following code.
```c#
using Blazor.Fluxor;

namespace CounterSample.Client.Store.Counter
{
	public class CounterFeature : Feature<CounterState>
	{
		public override string GetName() => "Counter";
		protected override CounterState GetInitialState() => new CounterState(0);
	}
}
```
   * Your class should descend from `Feature<>`, and the type parameter should specify the class you intend to use as the feature's state - in this case the `CounterState` class.
   * Your class should override the `GetName()` method. This should return a unique name used to store this feature's state in the store.
   * Your class should override the `GetInitialState()` method. This should return the initial state of this feature. This means whatever state you'd like this part of your application to contain *before* the user interacts with it.
 
### Displaying state in the user interface
1. Edit `Pages\Counter.razor` and add the following `using` clauses.
```c#
@using Blazor.Fluxor
@using Store.Counter
@inject IState<CounterState> CounterState
```
   * `@using Blazor.Fluxor` is required in order to identify the `IState<>` interface.
   * `@using Store.Counter` is required to identify the `CounterState` class we wish to use.
   * `@inject IState<CounterState> CounterState` will instruct Blazor to provide use with a reference to an interface from which we can obtain state.

2. Change the html that displays the value of the counter to display `@CounterState.Value.ClickCount` instead.

### Dispatching actions to mutate the state
The Flux pattern is structured so that the logic and state of the application can effectively function perfectly fine without a user interface being present at all. Changes to state are executed by using the `IDispatcher` service to dispatch an `Action` telling it what to do next (update a person's details, increment a counter, or something else).

In the Client project's `Store\Counter` folder create a class named `IncrementCounterAction.cs` and add the following code.
```c#
using Blazor.Fluxor;

namespace CounterSample.Client.Store.Counter
{
	public class IncrementCounterAction
	{
	}
}
```
   * In more complicated scenarios the action will have properties, in this case we don't need any as the action will always simply increment the current counter by 1.
3. We now need to dispatch an instance of this action to the store whenever the user clicks the `Click me` button. Inject the following dependency to the top of the `Pages\Counter.razor` file.
```c#
@inject IDispatcher Dispatcher
```
The declaration section at the top of the file should now look like this:
```c#
@page "/counter"
@using Blazor.Fluxor
@using Store.Counter
@inject IDispatcher Dispatcher
@inject IState<CounterState> CounterState
```
   * The `@using Store.Counter` line is needed to identify the `CounterState` and `IncrementCounterAction` classes.
   * The `@inject IDispatcher Dispatcher` line instructs Blazor to inject an object can use to dispatch actions through the store.
4. Change the `IncrementCount` function to dispatch an action through the store instructing it to increment the counter value.
```c#
void IncrementCount()
{
    Dispatcher.Dispatch(new IncrementCounterAction());
}
```
   
### Mutating the state in response to dispatched actions
So far we have state for our feature, a feature that exposes that state for displaying in the user interface, and an action we can dispatch through the store to indicate the user's desire to increment the value in the state. The final piece of the pattern is to implement a `Reducer`.

A `Reducer` is effectively a pure function. It takes the two parameters, the current state and the action dispatched. It then returns a new state according to the property values of the action (in this case there are no properties on the action, so our `Reducer` will always just increment the value by one).

If you recall, earlier I recommended you make all of the properties of your state have private setters. The recommended pattern for reducing (altering) your state is to replace it with a completely new object that is the same as the previous state but with the relevant changes to its values.

1. In the `Store\Counter` folder create a new file named `IncrementCounterReducer.cs` and enter the following code.
```c#
using Blazor.Fluxor;

namespace CounterSample.Client.Store.Counter.IncrementCounter
{
	public class IncrementCounterReducer : Reducer<CounterState, IncrementCounterAction>
	{
		public override CounterState Reduce(CounterState state, IncrementCounterAction action)
		{
			return new CounterState(state.ClickCount + 1);
		}
	}
}
```

   * We import the `Blazor.Fluxor` namespace in order to identify the `Reducer<TState, TAction>` interface.
   * The first generic parameter in the interface should identify the state the `Reducer` deals with, in this case `CounterState`.
   * The second generic parameter in the interface should identify the action the `Reducer` will react to, in this case it is the `IncrementCounterAction` class.
   * Note that the `Reducer<TState, TAction>` class is for convenience only, `Fluxor` actually works with any class that implements `IReducer<TState>`.
   
If the `Reducer` does not modify the state then you can simply return the original state object passed into the `Reduce` method.

Note that the folder structure and naming conventions used here are only recommendations. You may wish to have separate folders for Actions, Reducers, and Effects.