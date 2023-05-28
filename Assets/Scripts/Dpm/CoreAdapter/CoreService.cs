using System;
using Core.Base.Async;
using Core.Base.Message;
using Core.Base.Resource;
using Core.Base.Scene;
using Core.Base.Update;
using Core.Interface;
using UnityEngine;

namespace Dpm.CoreAdapter
{
	/// <summary>
	/// 코어 서비스 홀더
	/// 편의성을 위해 싱글톤 패턴을 사용했음
	/// </summary>
	public class CoreService : IDisposable, IUpdatable
	{
		private static CoreService _instance;

		public static ICoroutineManager Coroutine => _instance._coroutine;

		private ICoroutineManager _coroutine;

		public static IResourceManager Resource => _instance._resource;

		private IResourceManager _resource;

		public static IEventSystem Event => _instance._event;

		private IEventSystem _event;

		public static IUpdateSystem Update => _instance._update;

		private IUpdateSystem _update;

		public static ISceneManager Scene => _instance._scene;

		private ISceneManager _scene;

		public CoreService(MonoBehaviour parent)
		{
			_coroutine = new CoroutineManager(parent);
			_resource = new ResourceManager(_coroutine);
			_event = new EventSystem();
			_update = new UpdateSystem();
			_scene = new SceneManager();

			_instance = this;
		}

		public void UpdateFrame(float dt)
		{
			_event.ProcessEventRequests();
			_update.UpdateFrame(dt);
		}

		public void Dispose()
		{
			_scene.ExitCurrentScene();

			_event.Dispose();

			_update.Dispose();

			_resource.ClearAll();
			_resource.Dispose();

			_coroutine.Dispose();
		}
	}
}