using System.Collections;
using Core.Interface;
using Dpm.Common;
using Dpm.CoreAdapter;
using Dpm.MainMenu.Event;
using Dpm.Utility.Pool;
using UnityEngine;
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
			CoreService.Event.Subscribe<OptionButtonEvent>(OnOptionButton);

			ScreenTransition.Instance.FadeIn();
		}

		public void Exit()
		{
			ScreenTransition.Instance.FadeOut();

			CoreService.Event.Unsubscribe<ExitButtonEvent>(OnExitButton);
			CoreService.Event.Unsubscribe<StartButtonEvent>(OnStartButton);
			CoreService.Event.Unsubscribe<OptionButtonEvent>(OnOptionButton);
		}

		private void OnStartButton(IEvent e)
		{
			Game.Instance.MoveToStage();
		}

		private void OnOptionButton(IEvent e)
		{
			GameObjectPool.Get("test").TrySpawn(Vector3.zero, out _);
		}

		private void OnExitButton(IEvent e)
		{
			Game.Instance.ExitGame();
		}
	}
}