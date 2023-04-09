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

        public delegate bool Subscriber(Event e);

        // private struct IssuedEvent
        // {
        //     public Event Event;
        //     public IEventListener Listener;
        // }

        // private List<IssuedEvent> _issuedEvents;

        private Dictionary<Type, List<Subscriber>> _subscribers = new ();

        void IDisposable.Dispose()
        {
            _subscribers?.Clear();
            _subscribers = null;
        }

        void ISystem.Update()
        {
            // TODO : _issuedEvents 처리
        }

        public void Subscribe<T>(Subscriber subscriber) where T : Event
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
            if (subscribers.Contains(subscriber))
            {
                Debug.LogError($"{ subscriber } Has Subscribed { eventType } Already.");
                return;
            }
#endif
            subscribers.Add(subscriber);
        }

        public void Unsubscribe<T>(Subscriber subscriber) where T : Event
        {
            var eventType = typeof(T);

            if (!_subscribers.TryGetValue(eventType, out var subscribers))
            {
                return;
            }

            subscribers.Remove(subscriber);

            // 해당 Event에 대해 구독자가 더 이상 남아있지 않은 경우 리스트도 지워줌
            // FIXME : Any() 등의 함수를 사용하는 것과 어떤 것이 다른가?
            if (subscribers.Count == 0)
            {
                _subscribers.Remove(eventType);
            }
        }

        public bool SendImmediate(IEventListener listener, Event e, bool disposeAfter = true)
        {
            bool consumed = listener.OnEvent(e);

            if (disposeAfter)
            {
                e.Dispose();
            }

            return consumed;
        }

        public bool PublishImmediate(Event e, bool disposeAfter = true)
        {
            bool consumed = false;

            if (_subscribers.TryGetValue(e.GetType(), out var subscribers))
            {
                for (int i = 0; i < subscribers.Count; i++)
                {
                    // 흡수된 경우에는 Return해버림
                    consumed = subscribers[i](e);

                    if (consumed)
                    {
                        break;
                    }
                }
            }

            if (disposeAfter)
            {
                e.Dispose();
            }

            return consumed;
        }

        // public void Send(Event e)
        // {
        //
        // }
        //
        // public void Publish(Event e)
        // {
        //
        // }
    }
}