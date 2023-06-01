using Core.Interface;
using Core.Utility.Pool;

namespace Core.Utility.Event
{
	/// <summary>
	/// 풀링 가능한 추상 이벤트 클래스
	/// 다른 데이터의 같은 타입의 이벤트가 자주 발생하기 때문에, 이를 풀링하는 것이 유리
	/// </summary>
	/// <typeparam name="T">이 추상 클래스를 오버라이드하는 클래스</typeparam>
	public abstract class PooledEvent<T> : IEvent where T : PooledEvent<T>, new()
	{
		protected static InstancePool<T> Pool = new();

		public void Dispose()
		{
		}
	}
}