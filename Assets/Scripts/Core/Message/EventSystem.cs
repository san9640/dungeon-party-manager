using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Core.Message
{
	/// <summary>
	/// 메세지 시스템
	/// </summary>
	public class EventSystem : ISystem
	{
		public static EventSystem Instance { get; } = new();

		/// <summary>
		/// 구독자(함수)의 형태 규정
		/// </summary>
		public delegate bool Subscriber(Event e);

		/// <summary>
		/// (Immediate가 아닌 메소드로) 발행된 이벤트들
		/// </summary>
		private struct EventRequest
		{
			public Event Event;

			/// <summary>
			/// null이면 브로드캐스팅된 이벤트임
			/// </summary>
			public IEventListener Listener;

			/// <summary>
			/// 이 이벤트가 브로드캐스팅된 것이면 true
			/// </summary>
			public bool IsPublished => ReferenceEquals(Listener, null);
		}

		/// <summary>
		/// 구독인지 구독 해제인지 확인하기 위한 타입
		/// </summary>
		private enum SubscribeRequestType
		{
			Subscribe,
			Unsubscribe
		}

		private const int SubscriberWalkerInvalidValue = -1;
		private int _subscriberWalker = SubscriberWalkerInvalidValue;

		private bool IsSubscribeWalking => _subscriberWalker == SubscriberWalkerInvalidValue;

		private Type _subscriberWalkingEventType;

		private readonly Dictionary<Type, List<Subscriber>> _subscribers = new();

		private readonly List<EventRequest> _eventRequests = new();

		void IDisposable.Dispose()
		{
			_eventRequests.Clear();
			_subscribers.Clear();
		}

		void ISystem.Update()
		{
			// _eventRequests 처리
			for (int i = 0; i < _eventRequests.Count; i++)
			{
				var request = _eventRequests[i];

				if (request.IsPublished)
				{
					_subscriberWalkingEventType = request.Event.GetType();

					if (_subscribers.TryGetValue(_subscriberWalkingEventType, out var subscribers))
					{
						for (_subscriberWalker = 0; _subscriberWalker < subscribers.Count; _subscriberWalker++)
						{
							var consumed = subscribers[_subscriberWalker].Invoke(request.Event);

							if (consumed)
							{
								break;
							}
						}

						_subscriberWalker = SubscriberWalkerInvalidValue;
					}

					_subscriberWalkingEventType = null;
				}
				else
				{
					request.Listener.OnEvent(request.Event);
				}

				request.Event.Dispose();
			}

			_eventRequests.Clear();
		}

		/// <summary>
		/// 구독 요청
		/// </summary>
		/// <param name="target">구독할 대상(메서드). null을 넣지 말 것!!</param>
		/// <typeparam name="T">구독할 Event 타입</typeparam>
		public void Subscribe<T>(Subscriber target) where T : Event
		{
			var eventType = typeof(T);

			if (!_subscribers.TryGetValue(eventType, out var subscribers))
			{
				subscribers = new List<Subscriber>();

				_subscribers.Add(eventType, subscribers);
			}

#if UNITY_EDITOR
			// 에디터에서는 중복 구독을 개발 단계에서 캐치할 수 있도록 에러 로그를 남김
			// 빌드에서 체크하지 않는 이유는 List 순회에 비용이 크게 발생하기 때문
			// FIXME : 빌드에서는 중복 구독이 발생한다고 하면 어떤 방식으로 트래킹해야 하는가?
			if (subscribers.Contains(target))
			{
				Debug.LogError($"{target} Has Subscribed {eventType} Already.");
				return;
			}
#endif
			subscribers.Add(target);
		}

		/// <summary>
		/// 구독 해제 요청
		/// </summary>
		/// <param name="target">구독 해제할 대상(메서드)</param>
		/// <typeparam name="T">구독 해제할 Event 타입</typeparam>
		public void Unsubscribe<T>(Subscriber target) where T : Event
		{
			var eventType = typeof(T);

			if (!_subscribers.TryGetValue(eventType, out var subscribers))
			{
				return;
			}

			for (int i = 0; i < subscribers.Count; i++)
			{
				if (subscribers[i] != target)
					continue;

				// 해당 타입에 대해서 브로드캐스팅 중이면 인덱스 검사해서 조정해줌
				if (eventType == _subscriberWalkingEventType && IsSubscribeWalking && i > _subscriberWalker)
				{
					_subscriberWalker--;
				}

				subscribers.RemoveAt(i);

				break;
			}

			// 해당 Event에 대해 구독자가 더 이상 남아있지 않은 경우 리스트도 지워줌
			// FIXME : Any() 등의 함수를 사용하는 것과 어떤 것이 다른가?
			if (subscribers.Count == 0)
			{
				// 브로드캐스팅 문맥 상에서 동작하는 상황이라도, 레퍼런스는 해당 문맥이 들고있고 바로 파괴되는 것도 아니기 때문에 _subscribers에서는 지워도 됨
				_subscribers.Remove(eventType);
			}
		}

		/// <summary>
		/// 즉시 이벤트 전달
		/// </summary>
		/// <param name="listener">전달받을 객체</param>
		/// <param name="e">전달할 이벤트</param>
		/// <param name="disposeAfter">처리가 끝나고 Dispose를 해 줄 것인지. 최대한 기본값을 쓰는 것이 좋음</param>
		/// <returns>흡수되었는지 여부</returns>
		public bool SendImmediate(IEventListener listener, Event e, bool disposeAfter = true)
		{
			bool consumed = false;

			if (listener != null)
			{
				consumed = listener.OnEvent(e);
			}
			else
			{
				Debug.LogError($"Send an { e } to null target");
			}

			if (disposeAfter)
			{
				e.Dispose();
			}

			return consumed;
		}

		/// <summary>
		/// 이벤트 즉시 브로드캐스팅
		/// </summary>
		/// <param name="e">브로드캐스팅할 이벤트</param>
		/// <param name="disposeAfter">처리가 끝나고 Dispose를 해 줄 것인지. 최대한 기본값을 쓰는 것이 좋음</param>
		/// <returns>흡수되었는지 여부</returns>
		public bool PublishImmediate(Event e, bool disposeAfter = true)
		{
			bool consumed = false;

			_subscriberWalkingEventType = e.GetType();

			if (_subscribers.TryGetValue(_subscriberWalkingEventType, out var subscribers))
			{
				for (_subscriberWalker = 0; _subscriberWalker < subscribers.Count; _subscriberWalker++)
				{
					// 흡수된 경우에는 Return해버림
					consumed = subscribers[_subscriberWalker].Invoke(e);

					if (consumed)
					{
						break;
					}
				}

				_subscriberWalker = SubscriberWalkerInvalidValue;
			}

			_subscriberWalkingEventType = null;

			if (disposeAfter)
			{
				e.Dispose();
			}

			return consumed;
		}

		/// <summary>
		/// EventSystem 업데이트 시점에 맞춰 이벤트 전달
		/// </summary>
		/// <param name="listener">전달받을 객체</param>
		/// <param name="e">전달할 이벤트</param>
		public void Send(IEventListener listener, Event e)
		{
			if (listener != null)
			{
				_eventRequests.Add(new EventRequest
				{
					Event = e,
					Listener = listener
				});
			}
			else
			{
				Debug.LogError($"Send an { e } to null target");

				e.Dispose();
			}
		}

		/// <summary>
		/// EventSystem 업데이트 시점에 맞춰 이벤트 브로드캐스팅
		/// </summary>
		/// <param name="e">브로드캐스팅할 이벤트</param>
		public void Publish(Event e)
		{
			_eventRequests.Add(new EventRequest
			{
				Event = e,
				Listener = null
			});
		}
	}
}