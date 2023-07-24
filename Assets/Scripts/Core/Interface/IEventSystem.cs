using System;

namespace Core.Interface
{
	public interface IEventSystem : IDisposable
	{
		/// <summary>
		/// 구독자(함수)의 형태 규정
		/// </summary>
		/// <returns>Consume 여부</returns>
		public delegate void Subscriber(Event e);

		public void ProcessEventRequests();

		public void Subscribe<T>(Subscriber target) where T : Event;

		public void Unsubscribe<T>(Subscriber target) where T : Event;

		public void SendImmediate(IEventListener listener, Event e, bool disposeAfter = true);

		public void PublishImmediate(Event e, bool disposeAfter = true);

		public void Send(IEventListener listener, Event e);

		public void Publish(Event e);
	}
}