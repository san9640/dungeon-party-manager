using System;

namespace Core.Interface
{
	/// <summary>
	/// 주고 받을 이벤트.
	/// 이걸 직접 상속받게 하지 말고, PooledEvent 및 SingletonEvent를 상속받도록 하는 것이 좋음
	/// </summary>
	public interface IEvent : IDisposable
	{
	}
}