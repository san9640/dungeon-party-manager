using System;

namespace Core.Interface
{
	public interface IEventSystem : IDisposable
	{
		/// <summary>
		/// 구독자(함수)의 형태 규정
		/// </summary>
		/// <returns>Consume 여부</returns>
		public delegate void Subscriber(IEvent e);

		public void ProcessEventRequests();

		public void Subscribe<T>(Subscriber target) where T : IEvent;

		public void Unsubscribe<T>(Subscriber target) where T : IEvent;

		public void SendImmediate(IEventListener listener, IEvent e, bool disposeAfter = true);

		public void PublishImmediate(IEvent e, bool disposeAfter = true);

		public void Send(IEventListener listener, IEvent e);

		public void Publish(IEvent e);
	}
}