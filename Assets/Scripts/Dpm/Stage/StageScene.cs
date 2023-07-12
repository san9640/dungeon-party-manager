using System.Collections;
using Core.Interface;
using Dpm.Common;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using UnityScene = UnityEngine.SceneManagement.SceneManager;

namespace Dpm.Stage
{
	public class StageScene : IScene
	{
		private const string SceneName = "Stage";

		public IEnumerator LoadAsync()
		{
			yield return UnityScene.LoadSceneAsync(SceneName);
		}

		public void Enter()
		{
			CoreService.Event.Subscribe<ExitStageEvent>(OnExitStage);
		}

		public void Exit()
		{
			CoreService.Event.Unsubscribe<ExitStageEvent>(OnExitStage);
		}

		private void OnExitStage(IEvent e)
		{
			Game.Instance.MoveToMainMenu();
		}
	}
}