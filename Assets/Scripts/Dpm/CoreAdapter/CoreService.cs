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
	public class CoreService : IDisposable, IUpdatable, ILateUpdatable
	{
		private static CoreService _instance;

		public static ICoroutineManager Coroutine => _instance._coroutine;

		private ICoroutineManager _coroutine;

		/// <summary>
		/// AssetManager 인스턴스
		/// </summary>
		public static IAssetManager Asset => _instance._asset;

		private IAssetManager _asset;

		/// <summary>
		/// EventSystem 인스턴스
		/// </summary>
		public static IEventSystem Event => _instance._event;

		private IEventSystem _event;

		/// <summary>
		/// FrameUpdateSystem 인스턴스
		/// </summary>
		public static IFrameUpdateSystem FrameUpdate => _instance._frameUpdate;

		private IFrameUpdateSystem _frameUpdate;

		public static ISceneManager Scene => _instance._scene;

		private ISceneManager _scene;

		public CoreService(MonoBehaviour parent)
		{
			_coroutine = new CoroutineManager(parent);

			_asset = new AssetManager(
				"Assets/Resources/ScriptableObject/PrefabSpecs.asset",
				"Assets/Resources/ScriptableObject/ScriptableObjectSpecs.asset"
				);
			_event = new EventSystem();
			_frameUpdate = new FrameUpdateSystem();
			_scene = new SceneManager();

			_instance = this;
		}
		void IUpdatable.UpdateFrame(float dt)
		{
			_frameUpdate.UpdateFrame(dt);
			_event.ProcessEventRequests();
		}

		void ILateUpdatable.LateUpdateFrame(float dt)
		{
			_frameUpdate.LateUpdateFrame(dt);
		}

		public void OnSceneExited()
		{
			_event.Dispose();
			_frameUpdate.Dispose();
		}

		void IDisposable.Dispose()
		{
			_scene.ExitCurrentScene();
			_scene = null;

			_event.Dispose();
			_event = null;

			_frameUpdate.Dispose();
			_frameUpdate = null;

			_asset.Dispose();
			_asset = null;

			_coroutine.Dispose();
			_coroutine = null;
		}
	}
}