using System;

namespace Core.Interface
{
	public interface IEventSystem : IDisposable
	{
		/// <summary>
		/// 구독자(함수)의 형태 규정
		/// </summary>
		/// <returns>Consume 여부</returns>
		public delegate bool Subscriber(EventBase e);

		public void ProcessEventRequests();

		public void Subscribe<T>(Subscriber target) where T : EventBase;

		public void Unsubscribe<T>(Subscriber target) where T : EventBase;

		public bool SendImmediate(IEventListener listener, EventBase e, bool disposeAfter = true);

		public bool PublishImmediate(EventBase e, bool disposeAfter = true);

		public void Send(IEventListener listener, EventBase e);

		public void Publish(EventBase e);
	}
}