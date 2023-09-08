using System;

namespace Dpm.Utility.State
{
	public class StateMachine
	{
		public IState CurrentState { get; private set; }

		public void ChangeState(IState nextState)
		{
			CurrentState?.Exit();
			CurrentState?.Dispose();

			CurrentState = nextState;

			CurrentState.Enter();
		}
	}
}