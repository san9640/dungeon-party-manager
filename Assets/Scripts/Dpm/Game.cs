using System;
using Core;
using Core.Message;
using Core.Update;
using UnityEngine;

namespace Dpm
{
	public class Game : MonoBehaviour
	{
		public static Game Instance => _instance;

		private static Game _instance;

		private EventSystem _eventSystem;
		private UpdateSystem _updateSystem;

		private void Awake()
		{
			_instance = this;

			_eventSystem = EventSystem.Instance;
			_updateSystem = new();

			// 씬 이동이나 씬 전체 해제 등의 행위로 날아가지 않도록 세팅
			DontDestroyOnLoad(this);
		}

		private void OnDestroy()
		{
			(_eventSystem as IDisposable).Dispose();
			(_updateSystem as IDisposable).Dispose();
		}

		private void Update()
		{
			var dt = Time.deltaTime;

			// FIXME : 순서 지정 필요
			(_updateSystem as IUpdatable).UpdateFrame(dt);
			_eventSystem.ProcessEventRequests();
		}
	}
}