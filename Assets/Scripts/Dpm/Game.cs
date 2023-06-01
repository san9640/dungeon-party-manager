using Dpm.CoreAdapter;
using Dpm.MainMenu;
using Dpm.Utility.Pool;
using UnityEngine;

namespace Dpm
{
	public class Game : MonoBehaviour
	{
		public static Game Instance => _instance;

		private static Game _instance;

		private CoreService _service;

		private void Awake()
		{
			_instance = this;

			_service = new CoreService(this);

			// 씬 이동이나 씬 전체 해제 등의 행위로 날아가지 않도록 세팅
			DontDestroyOnLoad(this);

			CoreService.Coroutine.StartCoroutine(
				CoreService.Scene.EnterScene(new MainMenuScene()));
		}

		private void OnDestroy()
		{
			_service.Dispose();
			_service = null;

			InstancePool.Clear();

			_instance = null;
		}

		private void Update()
		{
			var dt = Time.deltaTime;

			_service.UpdateFrame(dt);
		}

		private void LateUpdate()
		{
			var dt = Time.deltaTime;

			_service.LateUpdateFrame(dt);
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