using System;
using Xunit;

namespace Blazor.Fluxor.UnitTests
{
	public class MultiActionReducerTests
	{

		public class ShouldReduceStateForAction
		{
			[Fact]
			public void ShouldReturnTrue_WhenReducerFunctionIsRegistered()
			{
				var testSubject = new MultiActionReducerTestSubject();

				var addAction = new AddAction(1);
				Assert.True(testSubject.ShouldReduceStateForAction(addAction));

				var multiplyAction = new MultiplyAction(1);
				Assert.True(testSubject.ShouldReduceStateForAction(multiplyAction));
			}

			[Fact]
			public void ShouldReturnFalse_WhenReducerFunctionIsNotRegistered()
			{
				var testSubject = new MultiActionReducerTestSubject();

				var unsupportedAction = new UnsupportedAction();
				Assert.False(testSubject.ShouldReduceStateForAction(unsupportedAction));
			}
		}

		public class Reduce
		{
			[Fact]
			public void ShouldExecuteReducerFunction_WhenReducerFunctionIsRegisteredForActionType()
			{
				var testSubject = new MultiActionReducerTestSubject();

				var state = new MultiActionReducerTestState(41);
				var addAction = new AddAction(1);
				state = testSubject.Reduce(state, addAction);
				Assert.Equal(42, state.Balance);

				state = new MultiActionReducerTestState(4);
				var multiplyAction = new MultiplyAction(10);
				state = testSubject.Reduce(state, multiplyAction);
				Assert.Equal(40, state.Balance);
			}

			[Fact]
			public void ShouldThrowInvalidOperationException_WhenNoReducerFunctionIsRegisteredForActionType()
			{
				var testSubject = new MultiActionReducerTestSubject();

				var state = new MultiActionReducerTestState(42);
				var unsupportedAction = new UnsupportedAction();
				Assert.Throws<InvalidOperationException>(() =>
					state = testSubject.Reduce(state, unsupportedAction));
			}
		}

		class MultiActionReducerTestState
		{
			public int Balance { get; }
			public MultiActionReducerTestState(int balance) => Balance = balance;
		}

		class AddAction
		{
			public int Amount { get; }
			public AddAction(int amount) => Amount = amount;
		}

		class MultiplyAction
		{
			public int Factor { get; }
			public MultiplyAction(int factor) => Factor = factor;
		}

		class UnsupportedAction
		{

		}

		class MultiActionReducerTestSubject : MultiActionReducer<MultiActionReducerTestState>
		{
			public MultiActionReducerTestSubject()
			{
				AddActionReducer<AddAction>((state, action) => 
					new MultiActionReducerTestState(state.Balance + action.Amount));

				AddActionReducer<MultiplyAction>((state, action) =>
					new MultiActionReducerTestState(state.Balance * action.Factor));
			}
		}
	}
}
