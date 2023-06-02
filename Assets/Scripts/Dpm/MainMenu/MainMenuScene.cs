using System.Collections;
using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.MainMenu.Event;
using Dpm.Stage;
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
			CoreService.Event.Subscribe<StartButtonEvent>(OnStartButton);
		}

		public void Exit()
		{
			CoreService.Event.Unsubscribe<ExitButtonEvent>(OnExitButton);
			CoreService.Event.Unsubscribe<StartButtonEvent>(OnStartButton);
		}

		private void OnStartButton(IEvent e)
		{
			Game.Instance.MoveToStage();
		}

		private void OnExitButton(IEvent e)
		{
			Game.Instance.ExitGame();
			// Game.Instance.ChangeScene(new MainMenuScene());
		}
	}
}