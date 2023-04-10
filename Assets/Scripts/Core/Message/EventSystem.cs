using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Message
{
	/// <summary>
	/// 메세지 시스템
	/// </summary>
	public class EventSystem : ISystem
	{
		public static EventSystem Instance { get; } = new EventSystem();

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

		/// <summary>
		/// 구독/구독 해제 요청 정보
		/// </summary>
		private struct SubscribeRequest
		{
			/// <summary>
			/// 구독/구독해제할 이벤트의 타입
			/// 이벤트의 타입은 항상 Core.Message.Event를 상속받고 있을 것이므로 (Subscribe를 통해 사용) 타입에 대한 제너릭은 정의하지 않음
			/// </summary>
			public Type EventType;

			/// <summary>
			/// 구독/구독해제할 콜백
			/// </summary>
			public Subscriber Target;

			/// <summary>
			/// 구독인지 구독 해제인지
			/// </summary>
			public SubscribeRequestType RequestType;
		}

		private List<SubscribeRequest> _subscribeRequests = new();

		private Dictionary<Type, List<Subscriber>> _subscribers = new();

		private List<EventRequest> _eventRequests = new();

		void IDisposable.Dispose()
		{
			_eventRequests.Clear();
			_subscribeRequests.Clear();
			_subscribers.Clear();
		}

		void ISystem.Update()
		{
			// 구독 / 구독 해제 요청 일괄 처리
			for (int i = 0; i < _subscribeRequests.Count; i++)
			{
				var request = _subscribeRequests[i];

				if (request.RequestType == SubscribeRequestType.Subscribe)
				{
					if (!_subscribers.TryGetValue(request.EventType, out var subscribers))
					{
						subscribers = new List<Subscriber>();

						_subscribers.Add(request.EventType, subscribers);
					}

#if UNITY_EDITOR
					// 에디터에서는 중복 구독을 개발 단계에서 캐치할 수 있도록 에러 로그를 남김
					// 빌드에서 체크하지 않는 이유는 List 순회에 비용이 크게 발생하기 때문
					// FIXME : 빌드에서는 중복 구독이 발생한다고 하면 어떤 방식으로 트래킹해야 하는가?
					if (subscribers.Contains(request.Target))
					{
						Debug.LogError($"{request.Target} Has Subscribed {request.EventType} Already.");
						return;
					}
#endif
					subscribers.Add(request.Target);
				}
				else
				{
					if (!_subscribers.TryGetValue(request.EventType, out var subscribers))
					{
						return;
					}

					subscribers.Remove(request.Target);

					// 해당 Event에 대해 구독자가 더 이상 남아있지 않은 경우 리스트도 지워줌
					// FIXME : Any() 등의 함수를 사용하는 것과 어떤 것이 다른가?
					if (subscribers.Count == 0)
					{
						_subscribers.Remove(request.EventType);
					}
				}
			}

			_subscribeRequests.Clear();

			// _eventRequests 처리
			for (int i = 0; i < _eventRequests.Count; i++)
			{
				var request = _eventRequests[i];

				if (request.IsPublished)
				{
					if (_subscribers.TryGetValue(request.Event.GetType(), out var subscribers))
					{
						for (int j = 0; j < subscribers.Count; j++)
						{
							bool hasUnsubscribedRequest = false;

							// 구독해제를 했는데 메세지가 전달되는 현상을 막기 위함
							// FIXME : 더 좋은 방법은 없을까?
							for (int k = 0; k < _subscribeRequests.Count; k++)
							{
								if (_subscribeRequests[i].RequestType == SubscribeRequestType.Unsubscribe &&
								    _subscribeRequests[i].Target == subscribers[i])
								{
									// 여기서 실시간으로 구독해제를 해버리면 구독 요청과의 싱크 문제가 있기 때문에 일단 패스
									hasUnsubscribedRequest = true;
									break;
								}
							}

							if (!hasUnsubscribedRequest)
							{
								var consumed = subscribers[i].Invoke(request.Event);

								if (consumed)
								{
									break;
								}
							}
						}
					}
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
			_subscribeRequests.Add(new SubscribeRequest
			{
				RequestType = SubscribeRequestType.Subscribe,
				Target = target,
				EventType = typeof(T)
			});
		}

		/// <summary>
		/// 구독 해제 요청
		/// </summary>
		/// <param name="target">구독 해제할 대상(메서드)</param>
		/// <typeparam name="T">구독 해제할 Event 타입</typeparam>
		public void Unsubscribe<T>(Subscriber target) where T : Event
		{
			_subscribeRequests.Add(new SubscribeRequest
			{
				RequestType = SubscribeRequestType.Unsubscribe,
				Target = target,
				EventType = typeof(T)
			});
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
			bool consumed = listener?.OnEvent(e) ?? false;

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

			if (_subscribers.TryGetValue(e.GetType(), out var subscribers))
			{
				for (int i = 0; i < subscribers.Count; i++)
				{
					bool hasUnsubscribedRequest = false;

					// 구독해제를 했는데 메세지가 전달되는 현상을 막기 위함
					// FIXME : 더 좋은 방법은 없을까?
					for (int k = 0; k < _subscribeRequests.Count; k++)
					{
						if (_subscribeRequests[i].RequestType == SubscribeRequestType.Unsubscribe &&
						    _subscribeRequests[i].Target == subscribers[i])
						{
							// 여기서 실시간으로 구독해제를 해버리면 구독 요청과의 싱크 문제가 있기 때문에 일단 패스
							hasUnsubscribedRequest = true;
							break;
						}
					}

					if (!hasUnsubscribedRequest)
					{
						// 흡수된 경우에는 Return해버림
						consumed = subscribers[i].Invoke(e);

						if (consumed)
						{
							break;
						}
					}
				}
			}

			if (disposeAfter)
			{
				e.Dispose();
			}

			return consumed;
		}

		/// <summary>
		/// EventSystem 업데이트 시점에 맞춰 이벤트 전달
		/// </summary>
		/// <param name="target">전달받을 객체</param>
		/// <param name="e">전달할 이벤트</param>
		public void Send(IEventListener target, Event e)
		{
			if (target == null)
			{
				return;
			}

			_eventRequests.Add(new EventRequest
			{
				Event = e,
				Listener = target
			});
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