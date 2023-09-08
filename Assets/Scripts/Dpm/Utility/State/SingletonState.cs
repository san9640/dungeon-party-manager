namespace Dpm.Utility.State
{
	public abstract class SingletonState<T> : IState where T : SingletonState<T>, new()
	{
		public static T Instance { get; private set; } = new T();

		public void Dispose()
		{
		}

		public void Enter()
		{
		}

		public void Exit()
		{
		}
	}
}