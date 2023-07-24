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

			GenerateField();
		}

		public void Enter()
		{
			CoreService.Event.Subscribe<ExitStageEvent>(OnExitStage);
			CoreService.Event.Subscribe<FieldClearedEvent>(OnFieldCleared);
		}

		public void Exit()
		{
			CoreService.Event.Unsubscribe<ExitStageEvent>(OnExitStage);
			CoreService.Event.Unsubscribe<FieldClearedEvent>(OnFieldCleared);

			// TODO : 그리드 옮겨갈 때마다 리셋해주는 것으로 변경
			_field.Dispose();

			Object.Destroy(_field.gameObject);

			_field = null;
		}

		private void OnExitStage(Core.Interface.Event e)
		{
			Game.Instance.MoveToMainMenu();
		}

		private void OnFieldCleared(Core.Interface.Event e)
		{
			CoreService.Coroutine.StartCoroutine(MoveFieldAsync());
		}

		private IEnumerator MoveFieldAsync()
		{
			yield return ScreenTransition.Instance.FadeOutAsync(1);

			_field.Dispose();

			GenerateField();

			yield return ScreenTransition.Instance.FadeInAsync(1);
		}

		private void GenerateField()
		{
			var doorCount = Random.Range(1, GameField.MaxDoorCount + 1);
			_field.Initialize(doorCount);
		}
	}
}