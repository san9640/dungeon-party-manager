using System;

namespace Dpm.Utility.State
{
	public interface IState : IDisposable
	{
		void Enter();
		void Exit();
	}
}