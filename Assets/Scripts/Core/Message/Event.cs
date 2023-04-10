using System;

namespace Core.Message
{
	/// <summary>
	/// 주고 받을 이벤트.
	/// 이걸 직접 상속받게 하지 말고, PooledEvent 및 SingletonEvent를 상속받도록 하는 것이 좋음
	/// </summary>
	public abstract class Event : IDisposable
	{
		public abstract void Dispose();
	}
}