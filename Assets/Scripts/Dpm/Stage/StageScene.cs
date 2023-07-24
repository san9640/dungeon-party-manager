using System.Collections;
using Core.Interface;
using Dpm.Common;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Field;
using UnityEngine;
using UnityScene = UnityEngine.SceneManagement.SceneManager;

namespace Dpm.Stage
{
	public class StageScene : IScene
	{
		private const string SceneName = "Stage";

		private GameField _field;

		public IEnumerator LoadAsync()
		{
			yield return UnityScene.LoadSceneAsync(SceneName);

			if (!CoreService.Asset.TryGet<GameObject>("field", out var prefab))
			{
				yield break;
			}

			var fieldGo = Object.Instantiate(prefab);

			_field = fieldGo.GetComponent<GameField>();

			// TODO : 그리드 옮겨갈 때마다 리셋해주는 것으로 변경
			_field.Initialize(5);
		}

		public void Enter()
		{
			CoreService.Event.Subscribe<ExitStageEvent>(OnExitStage);
		}

		public void Exit()
		{
			CoreService.Event.Unsubscribe<ExitStageEvent>(OnExitStage);

			// TODO : 그리드 옮겨갈 때마다 리셋해주는 것으로 변경
			_field.Dispose();

			Object.Destroy(_field.gameObject);

			_field = null;
		}

		private void OnExitStage(Core.Interface.Event e)
		{
			Game.Instance.MoveToMainMenu();
		}
	}
}