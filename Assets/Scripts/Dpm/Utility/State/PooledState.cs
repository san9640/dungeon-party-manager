using Dpm.Utility.Pool;

namespace Dpm.Utility.State
{
	/// <summary>
	/// 풀링 가능한 추상 스테이트 클래스
	/// </summary>
	public abstract class PooledState<T> : IState where T : PooledState<T>, new()
	{
		protected static readonly InstancePool<T> Pool = new();

		public virtual void Dispose()
		{
			Pool.Return(this as T);
		}

		public virtual void Enter()
		{
		}

		public virtual void Exit()
		{
		}
	}
}