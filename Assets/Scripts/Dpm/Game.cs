using System;
using System.Collections;
using Core.Interface;
using Dpm.Common;
using Dpm.CoreAdapter;
using Dpm.MainMenu;
using Dpm.Stage;
using Dpm.Stage.Spec;
using Dpm.Utility.Pool;
using UnityEngine;

namespace Dpm
{
	public class Game : MonoBehaviour, IDisposable
	{
		public static Game Instance => _instance;

		private static Game _instance;

		private CoreService _service;

		private enum StartSceneType
		{
			Default,
			MainMenu,
			Stage,
		}

		[SerializeField]
		private StartSceneType startSceneType = StartSceneType.Default;

		[SerializeField]
		private string[] assetSpecsHolderPaths;

		private bool _isSceneChanging = false;

		public StageScene Stage
		{
			get
			{
				if (CoreService.Scene.CurrentScene is StageScene stage)
				{
					return stage;
				}

				return null;
			}
		}

		public MainMenuScene MainMenu
		{
			get
			{
				if (CoreService.Scene.CurrentScene is MainMenuScene mainMenu)
				{
					return mainMenu;
				}

				return null;
			}
		}

		private void Awake()
		{
#if UNITY_EDITOR
			var gameStartTime = Time.realtimeSinceStartup;
#endif

			_instance = this;

			_service = new CoreService(this, assetSpecsHolderPaths);

			SpecUtility.CacheSpecData();

			// 씬 이동이나 씬 전체 해제 등의 행위로 날아가지 않도록 세팅
			DontDestroyOnLoad(this);

#if UNITY_EDITOR
			Debug.Log($"Load duration before launch : {Time.realtimeSinceStartup - gameStartTime}");
#endif

			CoreService.Coroutine.StartCoroutine(ActivateGame());
		}

		private IEnumerator ActivateGame()
		{
			_isSceneChanging = true;

			yield return null;

			IScene scene = startSceneType switch
			{
				StartSceneType.MainMenu => new MainMenuScene(),
				StartSceneType.Stage => new StageScene(),
				_ => new MainMenuScene()
			};

#if UNITY_EDITOR
			var sceneChangeStartTime = Time.realtimeSinceStartup;
#endif
			yield return CoreService.Scene.EnterScene(scene);

#if UNITY_EDITOR
			Debug.Log($"Scene change duration : {Time.realtimeSinceStartup - sceneChangeStartTime}");
#endif

			yield return ScreenTransition.Instance.FadeInAsync(1f, this);

			_isSceneChanging = false;
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
			if (_isSceneChanging)
			{
				return;
			}

			_isSceneChanging = true;

			CoreService.Coroutine.StartCoroutine(ChangeSceneAsync(next));
		}

		private IEnumerator ChangeSceneAsync(IScene next)
		{
			yield return ScreenTransition.Instance.FadeOutAsync(1f, this);

#if UNITY_EDITOR
			var sceneChangeStartTime = Time.realtimeSinceStartup;
#endif

			CoreService.Scene.ExitCurrentScene();

			_service.OnSceneExited();
			GameObjectPool.Dispose();
			InstancePool.Dispose();

			yield return CoreService.Scene.EnterScene(next);

#if UNITY_EDITOR
			Debug.Log($"Scene change duration : {Time.realtimeSinceStartup - sceneChangeStartTime}");
#endif

			yield return ScreenTransition.Instance.FadeInAsync(1f, this);

			_isSceneChanging = false;
		}
	}
}