using System;
using System.Collections;
using Core.Message;
using Core.Resource;
using Core.Async;
using Core.Update;
using Dpm.MainMenu;
using UnityEngine;

namespace Dpm
{
	public class Game : MonoBehaviour
	{
		public static Game Instance => _instance;

		private static Game _instance;

		public EventSystem EventSystem { get; private set; }

		public UpdateSystem UpdateSystem { get; private set; }

		public CoroutineManager CoroutineManager { get; private set; }

		public ResourceManager ResourceManager { get; private set; }

		public SceneManager SceneManager { get; private set; }

		private void Awake()
		{
			_instance = this;

			EventSystem = new();
			UpdateSystem = new();
			CoroutineManager = new(this);
			ResourceManager = new(CoroutineManager);

			SceneManager = new();

			// 씬 이동이나 씬 전체 해제 등의 행위로 날아가지 않도록 세팅
			DontDestroyOnLoad(this);

			CoroutineManager.StartCoroutine(
				SceneManager.EnterScene(new MainMenuScene()));
		}

		private void OnDestroy()
		{
			SceneManager.ExitCurrentScene();

			ResourceManager.ClearAll();
			(ResourceManager as IDisposable).Dispose();

			(CoroutineManager as IDisposable).Dispose();

			(UpdateSystem as IDisposable).Dispose();

			(EventSystem as IDisposable).Dispose();
		}

		private void Update()
		{
			var dt = Time.deltaTime;

			// FIXME : 순서 지정 필요
			(UpdateSystem as IUpdatable).UpdateFrame(dt);
			EventSystem.ProcessEventRequests();
		}

		public void ExitGame()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}