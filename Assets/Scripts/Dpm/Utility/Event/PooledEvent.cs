using Core.Interface;
using Dpm.Utility.Pool;

namespace Dpm.Utility.Event
{
	/// <summary>
	/// 풀링 가능한 추상 이벤트 클래스
	/// 다른 데이터의 같은 타입의 이벤트가 자주 발생하기 때문에, 이를 풀링하는 것이 유리
	/// </summary>
	/// <typeparam name="T">이 추상 클래스를 오버라이드하는 클래스</typeparam>
	public abstract class PooledEvent<T> : Core.Interface.Event where T : PooledEvent<T>, new()
	{
		protected static readonly InstancePool<T> Pool = new();

		public override void Dispose()
		{
			Pool.Return(this as T);
		}
	}
}