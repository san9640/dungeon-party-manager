using System.Collections;
using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.MainMenu.Event;
using UnityScene = UnityEngine.SceneManagement.SceneManager;

namespace Dpm.MainMenu
{
	public class MainMenuScene : IScene
	{
		private const string SceneName = "MainMenu";

		public IEnumerator LoadAsync()
		{
			yield return UnityScene.LoadSceneAsync(SceneName);
		}

		public void Enter()
		{
			CoreService.Event.Subscribe<ExitButtonEvent>(OnExitButton);
		}

		public void Exit()
		{
			CoreService.Event.Unsubscribe<ExitButtonEvent>(OnExitButton);
		}

		private bool OnExitButton(EventBase e)
		{
			Game.Instance.ExitGame();

			return false;
		}
	}
}