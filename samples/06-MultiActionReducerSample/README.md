# 06-MultiActionReducer sample
This sample shows how to have multiple reducer actions in a single reducer class.
This sample is based on [Tutorial 2].

## Setting up the project
To get started, follow the steps in [Tutorial 2] first.
  
## Moving reducers to a single class
First, create a file `Store\FetchData\FetchDataReducers.cs`.

Create a class that descends from `MultiActionReducer<TState>`, where `TState` is the state we wish to use, which in this case is `FetchDataState`.

```
public class FetchDataReducers : MultiActionReducer<FetchDataState>
{
}
```

Now open each of the `\*Reducer.cs` files in `Store\FetchData`.
- GetForecastDataActionReducer.cs
- GetForecastDataFailedActionReducer.cs
- GetForecastDataSuccessActionReducer.cs

In each case, add the code in the `Reduce(state, action)` method into
the the multi-action reducer's constructor like so:

### Before
```
public override FetchDataState Reduce(FetchDataState state, GetForecastDataAction action)
{
	return new FetchDataState(
		isLoading: true,
		errorMessage: null,
		forecasts: null);
}
```

### After
```
public FetchDataReducers()
{
	AddActionReducer<GetForecastDataAction>((state, action) =>
		new FetchDataState(
		isLoading: true,
		errorMessage: null,
		forecasts: null));
}
```

### Final reducer code
Delete the original reducers in `Store\FetchData`, and your `FetchDataReducers.cs` file should
finally look like this:

```
using Blazor.Fluxor;

namespace MultiActionReducerSample.Client.Store.FetchData
{
	public class FetchDataReducers : MultiActionReducer<FetchDataState>
	{
		public FetchDataReducers()
		{
			AddActionReducer<GetForecastDataAction>((state, action) =>
				new FetchDataState(
				isLoading: true,
				errorMessage: null,
				forecasts: null));

			AddActionReducer<GetForecastDataFailedAction>((state, action) =>
				new FetchDataState(
					isLoading: false,
					errorMessage: action.ErrorMessage,
					forecasts: null));

			AddActionReducer<GetForecastDataSuccessAction>((state, action) =>
				new FetchDataState(
					isLoading: false,
					errorMessage: null,
					forecasts: action.WeatherForecasts));
		}
	}
}
```

## Conclusion
The `MultiActionReducer<T>` is a very useful pattern to implement if your reducer code is
typically very simple.

Once the reducer code starts to become large you should consider extracting the larger reducer
functions into their own classes using the `Reducer<TState, TAction` class.


[Tutorial 2]: <https://github.com/mrpmorris/blazor-fluxor/tree/master/samples/02-WeatherForecastSample>
