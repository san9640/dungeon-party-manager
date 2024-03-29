﻿using System;
using System.Collections.Generic;
using Core.Interface;
using Event = Core.Interface.Event;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Core.Base.Message
{
	/// <summary>
	/// 이벤트를 주고받거나, 브로드캐스팅되는 이벤트를 구독/구독해제하는 시스템
	/// </summary>
	public class EventSystem : IEventSystem
	{
		/// <summary>
		/// (Immediate가 아닌 메소드로) 발행된 이벤트들
		/// </summary>
		private readonly struct EventRequest
		{
			public readonly Event Event;

			/// <summary>
			/// null이면 브로드캐스팅된 이벤트임
			/// </summary>
			public readonly IEventListener Listener;

			/// <summary>
			/// 이 이벤트가 브로드캐스팅된 것이면 true
			/// </summary>
			public bool IsPublished => ReferenceEquals(Listener, null);

			public EventRequest(Event e, IEventListener listener)
			{
				Event = e;
				Listener = listener;
			}
		}

		// -1이 아닌 이유는 0번째 Publish를 처리 중이었을 때, 순간적으로 Walker가 -1이 되는 시점이 있기 때문
		private const int SubscriberWalkerInvalidValue = -2;

		private int _subscriberWalker = SubscriberWalkerInvalidValue;

		private bool IsSubscribeWalking => _subscriberWalker != SubscriberWalkerInvalidValue;

		private Type _subscriberWalkingEventType;

		private readonly Dictionary<Type, List<IEventSystem.Subscriber>> _subscribers = new(1024);

		private readonly List<EventRequest> _eventRequests = new(1024);

		void IDisposable.Dispose()
		{
			foreach (var request in _eventRequests)
			{
				request.Event.Dispose();
			}

			_eventRequests.Clear();
			_subscribers.Clear();

			_subscriberWalkingEventType = null;
			_subscriberWalker = SubscriberWalkerInvalidValue;
		}

		/// <summary>
		/// Send / Publish될 이벤트를 일괄 처리. 매 프레임 호출 요망
		/// </summary>
		public void ProcessEventRequests()
		{
			// _eventRequests 처리
			for (var i = 0; i < _eventRequests.Count; i++)
			{
				var request = _eventRequests[i];

				if (request.IsPublished)
				{
					_subscriberWalkingEventType = _eventRequests[i].Event.GetType();

					if (_subscribers.TryGetValue(_subscriberWalkingEventType, out var subscribers))
					{
						for (_subscriberWalker = 0; _subscriberWalker < subscribers.Count; _subscriberWalker++)
						{
							subscribers[_subscriberWalker].Invoke(request.Event);

							// 메세지 처리 중 EventSystem이 해제되면 바로 중단
							if (!IsSubscribeWalking)
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
		public void Subscribe<T>(IEventSystem.Subscriber target) where T : Event
		{
			var eventType = typeof(T);

			if (!_subscribers.TryGetValue(eventType, out var subscribers))
			{
				subscribers = new List<IEventSystem.Subscriber>();

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
		public void Unsubscribe<T>(IEventSystem.Subscriber target) where T : Event
		{
			var eventType = typeof(T);

			if (!_subscribers.TryGetValue(eventType, out var subscribers))
			{
				return;
			}

			for (var i = 0; i < subscribers.Count; i++)
			{
				if (subscribers[i] != target)
					continue;

				// 지워진 콜백이 브로드캐스팅 처리 인덱스보다 크다면 walker 조정하지 않아도 됨
				if (eventType == _subscriberWalkingEventType && IsSubscribeWalking && i <= _subscriberWalker)
				{
					_subscriberWalker--;
				}

				subscribers.RemoveAt(i);

				break;
			}

			// 해당 Event에 대해 구독자가 더 이상 남아있지 않은 경우 리스트도 지워줌
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
		public void SendImmediate(IEventListener listener, Event e, bool disposeAfter = true)
		{
			if (listener != null)
			{
				listener.OnEvent(e);
			}
#if UNITY_EDITOR
			else
			{
				Debug.LogError($"Send an { e } to null target");
			}
#endif

			if (disposeAfter)
			{
				e.Dispose();
			}
		}

		/// <summary>
		/// 이벤트 즉시 브로드캐스팅
		/// </summary>
		/// <param name="e">브로드캐스팅할 이벤트</param>
		/// <param name="disposeAfter">처리가 끝나고 Dispose를 해 줄 것인지. 최대한 기본값을 쓰는 것이 좋음</param>
		/// <returns>흡수되었는지 여부</returns>
		public void PublishImmediate(Event e, bool disposeAfter = true)
		{
			_subscriberWalkingEventType = e.GetType();

			if (_subscribers.TryGetValue(_subscriberWalkingEventType, out var subscribers))
			{
				for (_subscriberWalker = 0; _subscriberWalker < subscribers.Count; _subscriberWalker++)
				{
					subscribers[_subscriberWalker].Invoke(e);

					// 메세지 처리 중 EventSystem이 해제되면 바로 중단
					if (!IsSubscribeWalking)
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
				_eventRequests.Add(new EventRequest(e, listener));
			}
#if UNITY_EDITOR
			else
			{
				Debug.LogError($"Send an { e } to null target");

				e.Dispose();
			}
#endif
		}

		/// <summary>
		/// EventSystem 업데이트 시점에 맞춰 이벤트 브로드캐스팅
		/// </summary>
		/// <param name="e">브로드캐스팅할 이벤트</param>
		public void Publish(Event e)
		{
			_eventRequests.Add(new EventRequest(e, null));
		}
	}
}