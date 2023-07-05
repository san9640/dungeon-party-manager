using System;
using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.MainMenu;
using Dpm.Stage;
using Dpm.Utility.Pool;
using UnityEngine;

namespace Dpm
{
	public class Game : MonoBehaviour, IDisposable
	{
		public static Game Instance => _instance;

		private static Game _instance;

		private CoreService _service;

		[SerializeField]
		private string[] assetSpecsHolderPaths;

		private void Awake()
		{
			_instance = this;

			_service = new CoreService(this, assetSpecsHolderPaths);

			// 씬 이동이나 씬 전체 해제 등의 행위로 날아가지 않도록 세팅
			DontDestroyOnLoad(this);

			CoreService.Coroutine.StartCoroutine(
				CoreService.Scene.EnterScene(new MainMenuScene()));
		}

		public void Dispose()
		{
			(_service as IDisposable).Dispose();
			_service = null;

			GameObjectPool.Dispose();
			InstancePool.Dispose();

			_instance = null;

			if (gameObject != null)
			{
				Destroy(gameObject);
			}
		}

		private void Update()
		{
			var dt = Time.deltaTime;

			(_service as IUpdatable).UpdateFrame(dt);
		}

		private void LateUpdate()
		{
			var dt = Time.deltaTime;

			(_service as ILateUpdatable).LateUpdateFrame(dt);
		}

		public void ExitGame()
		{
			Dispose();

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		public void MoveToStage()
		{
			ChangeScene(new StageScene());
		}

		public void MoveToMainMenu()
		{
			ChangeScene(new MainMenuScene());
		}

		private void ChangeScene(IScene next)
		{
			CoreService.Scene.ExitCurrentScene();

			_service.OnSceneExited();
			GameObjectPool.Dispose();
			InstancePool.Dispose();

			CoreService.Coroutine.StartCoroutine(CoreService.Scene.EnterScene(next));
		}
	}
}