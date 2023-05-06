using System.Collections;
using Dpm.MainMenu.Event;
using SceneManagement = UnityEngine.SceneManagement.SceneManager;

namespace Dpm.MainMenu
{
	public class MainMenuScene : IScene
	{
		// private GameObject _menuGo;

		private const string SceneName = "MainMenu";

		public IEnumerator LoadAsync()
		{
			// GameObject prefab = null;
			//
			// yield return Game.Instance.ResourceManager.
			// 	LoadAsync<GameObject>("Prefabs/MainMenu", go => prefab = go);
			//
			// _menuGo = Object.Instantiate(prefab, null);
			// _menuGo.SetActive(false);

			yield return SceneManagement.LoadSceneAsync(SceneName);
		}

		public void Enter()
		{
			// _menuGo.SetActive(true);

			Game.Instance.EventSystem.Subscribe<ExitButtonEvent>(OnExitButton);
		}

		public void Exit()
		{
			Game.Instance.EventSystem.Unsubscribe<ExitButtonEvent>(OnExitButton);

			// Object.Destroy(_menuGo);
			//
			// _menuGo = null;
		}

		private bool OnExitButton(Core.Message.Event e)
		{
			Game.Instance.ExitGame();

			return false;
		}
	}
}