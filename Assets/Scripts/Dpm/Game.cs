using System;
using Core.Message;
using Core.Resource;
using Core.Update;
using UnityEngine;

namespace Dpm
{
	public class Game : MonoBehaviour
	{
		public static Game Instance => _instance;

		private static Game _instance;

		private readonly EventSystem _eventSystem = EventSystem.Instance;

		public UpdateSystem UpdateSystem { get; private set; } = new();

		public ResourceManager ResourceManager { get; private set; } = new ResourceManager();

		private void Awake()
		{
			_instance = this;

			// 씬 이동이나 씬 전체 해제 등의 행위로 날아가지 않도록 세팅
			DontDestroyOnLoad(this);
		}

		private void OnDestroy()
		{
			(_eventSystem as IDisposable).Dispose();
			(UpdateSystem as IDisposable).Dispose();
		}

		private void Update()
		{
			var dt = Time.deltaTime;

			// FIXME : 순서 지정 필요
			(UpdateSystem as IUpdatable).UpdateFrame(dt);
			_eventSystem.ProcessEventRequests();
		}
	}
}