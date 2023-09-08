using System.Collections;
using Core.Interface;
using Dpm.Common;
using Dpm.Common.Event;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Room;
using Dpm.Stage.Unit;
using UnityEngine;
using UnityScene = UnityEngine.SceneManagement.SceneManager;

namespace Dpm.Stage
{
	public enum StageState
	{
		None,
		Loading,
		WaitBattle,
		InBattle,
		AfterBattle,
	}

	/// <summary>
	/// FIXME: 로드하면 이벤트 등을 받아 units에 추가되는 방식으로 변경 필요
	/// </summary>
	public class StageScene : IScene
	{
		private const string SceneName = "Stage";

		private GameRoom _field;

		public StageState State { get; private set; } = StageState.None;

		public StagePhysicsManager PhysicsManager { get; private set; }

		public UnitManager UnitManager { get; private set; }

		public IEnumerator LoadAsync()
		{
			yield return UnityScene.LoadSceneAsync(SceneName);

			PhysicsManager = new StagePhysicsManager();
			UnitManager = new UnitManager();

			if (!CoreService.Asset.TryGet<GameObject>("field", out var prefab))
			{
				yield break;
			}

			var fieldGo = Object.Instantiate(prefab);

			_field = fieldGo.GetComponent<GameRoom>();

			GenerateField();

			// FIXME : Register 방식을 변경하고 싶음
			foreach (var unit in _field.Units)
			{
				UnitManager.RegisterUnit(unit);
			}

			UnitManager.SpawnAllies(_field.AllySpawnArea);
		}

		public void Enter()
		{
			CoreService.Event.PublishImmediate(StageEnterEvent.Instance);

			CoreService.Event.Subscribe<ExitStageEvent>(OnExitStage);
			CoreService.Event.Subscribe<FieldClearedEvent>(OnFieldCleared);
			CoreService.Event.Subscribe<ScreenFadeOutStartEvent>(OnScreenFadeOutStart);
			CoreService.Event.Subscribe<ScreenFadeInEndEvent>(OnScreenFadeInEnd);
		}

		public void Exit()
		{
			CoreService.Event.Unsubscribe<ExitStageEvent>(OnExitStage);
			CoreService.Event.Unsubscribe<FieldClearedEvent>(OnFieldCleared);
			CoreService.Event.Unsubscribe<ScreenFadeOutStartEvent>(OnScreenFadeOutStart);
			CoreService.Event.Unsubscribe<ScreenFadeInEndEvent>(OnScreenFadeInEnd);

			CoreService.Event.PublishImmediate(StageExitEvent.Instance);

			UnitManager.Dispose();
			UnitManager = null;

			if (_field != null)
			{
				// TODO : 그리드 옮겨갈 때마다 리셋해주는 것으로 변경
				_field.Dispose();

				Object.Destroy(_field.gameObject);
			}

			_field = null;

			PhysicsManager?.Dispose();
			PhysicsManager = null;
		}

		private void OnExitStage(Core.Interface.Event e)
		{
			Game.Instance.MoveToMainMenu();
		}

		private void OnFieldCleared(Core.Interface.Event e)
		{
			CoreService.Coroutine.StartCoroutine(MoveRoomAsync());
		}

		private void OnScreenFadeOutStart(Core.Interface.Event e)
		{
			if (e is not ScreenFadeOutStartEvent se)
			{
				return;
			}

			// 요청자가 자신일 때에는 커스텀하게 작동하게 하는 것이 편하므로, Requester가 Game이거나 할 때에만 처리
			if (se.Requester != this)
			{
				State = StageState.Loading;
			}
		}

		private void OnScreenFadeInEnd(Core.Interface.Event e)
		{
			if (e is not ScreenFadeInEndEvent se)
			{
				return;
			}

			// 요청자가 자신일 때에는 커스텀하게 작동하게 하는 것이 편하므로, Requester가 Game이거나 할 때에만 처리
			if (se.Requester != this)
			{
				if (State == StageState.Loading)
				{
					State = StageState.WaitBattle;
				}
			}
		}

		/// <summary>
		/// 클리어 이후 방을 이동하는 코루틴
		/// </summary>
		private IEnumerator MoveRoomAsync()
		{
			State = StageState.Loading;

			yield return ScreenTransition.Instance.FadeOutAsync(1, this);

			CoreService.Event.PublishImmediate(RoomChangeStartEvent.Instance);

			_field.Dispose();

			GenerateField();

			CoreService.Event.PublishImmediate(RoomChangeEndEvent.Instance);

			yield return ScreenTransition.Instance.FadeInAsync(1, this);

			State = StageState.WaitBattle;
		}

		/// <summary>
		/// Field 생성
		/// </summary>
		private void GenerateField()
		{
			// FIXME
			var doorCount = Random.Range(1, GameRoom.MaxDoorCount + 1);

			_field.Initialize(doorCount);
		}
	}
}