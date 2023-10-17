using System.Collections;
using Core.Interface;
using Dpm.Common;
using Dpm.Common.Event;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Room;
using Dpm.Stage.UI;
using Dpm.Stage.UI.Event;
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
		public static StageScene Instance => Game.Instance.Stage;

		private const string SceneName = "Stage";

		private GameRoom _room;

		public StageState State { get; private set; } = StageState.None;

		public StagePhysicsManager PhysicsManager { get; private set; }

		public UnitManager UnitManager { get; private set; }

		private StageUIManager _stageUIManager;

		public ProjectileManager ProjectileManager { get; private set; }

		public IEnumerator LoadAsync()
		{
			State = StageState.Loading;

			yield return UnityScene.LoadSceneAsync(SceneName);

			PhysicsManager = new StagePhysicsManager();
			UnitManager = new UnitManager();

			var prefab = CoreService.Asset.UnsafeGet<GameObject>("field");
			var roomGo = Object.Instantiate(prefab);

			_room = roomGo.GetComponent<GameRoom>();

			GenerateRoom();

			// FIXME : Register 방식을 변경하고 싶음
			foreach (var unit in _room.Units)
			{
				unit.Region = UnitRegion.Neutral;
				UnitManager.RegisterUnit(unit);
			}

			UnitManager.SpawnAllies(_room.AllySpawnArea);
			UnitManager.SpawnEnemies(_room.EnemySpawnArea);

			ProjectileManager = new ProjectileManager();

			_stageUIManager = StageUIManager.Instance;
			_stageUIManager.Init();
		}

		public void Enter()
		{
			CoreService.Event.PublishImmediate(StageEnterEvent.Instance);

			CoreService.Event.Subscribe<ExitStageEvent>(OnExitStage);
			CoreService.Event.Subscribe<RoomClearedEvent>(OnRoomCleared);
			CoreService.Event.Subscribe<ScreenFadeOutStartEvent>(OnScreenFadeOutStart);
			CoreService.Event.Subscribe<ScreenFadeInEndEvent>(OnScreenFadeInEnd);
			CoreService.Event.Subscribe<PartyEliminatedEvent>(OnPartyEliminated);
			CoreService.Event.Subscribe<BattleStartButtonPressedEvent>(OnBattleStartButtonPressed);
		}

		public void Exit()
		{
			CoreService.Event.Unsubscribe<ExitStageEvent>(OnExitStage);
			CoreService.Event.Unsubscribe<RoomClearedEvent>(OnRoomCleared);
			CoreService.Event.Unsubscribe<ScreenFadeOutStartEvent>(OnScreenFadeOutStart);
			CoreService.Event.Unsubscribe<ScreenFadeInEndEvent>(OnScreenFadeInEnd);
			CoreService.Event.Unsubscribe<PartyEliminatedEvent>(OnPartyEliminated);
			CoreService.Event.Unsubscribe<BattleStartButtonPressedEvent>(OnBattleStartButtonPressed);

			CoreService.Event.PublishImmediate(StageExitEvent.Instance);

			_stageUIManager.Dispose();
			_stageUIManager = null;

			UnitManager.Dispose();
			UnitManager = null;

			if (_room != null)
			{
				// TODO : 그리드 옮겨갈 때마다 리셋해주는 것으로 변경
				_room.Dispose();

				Object.Destroy(_room.gameObject);
			}

			_room = null;

			PhysicsManager?.Dispose();
			PhysicsManager = null;
		}

		private void OnExitStage(Core.Interface.Event e)
		{
			Game.Instance.MoveToMainMenu();
		}

		private void OnRoomCleared(Core.Interface.Event e)
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

		private void OnBattleStartButtonPressed(Core.Interface.Event e)
		{
			if (e is not BattleStartButtonPressedEvent bsbe)
			{
				return;
			}

			State = StageState.InBattle;

			CoreService.Event.Publish(BattleStartEvent.Instance);

			bsbe.DeactivateButton();
		}

		private void OnPartyEliminated(Core.Interface.Event e)
		{
			if (e is not PartyEliminatedEvent pee)
			{
				return;
			}

			if (pee.Party.Region == UnitRegion.Ally)
			{
				State = StageState.AfterBattle;

				CoreService.Event.Publish(BattleEndEvent.Create(UnitRegion.Enemy));

				// TODO : 임시 처리임. 게임 결과창 띄워주기
				Game.Instance.MoveToMainMenu();
			}
			else if (pee.Party.Region == UnitRegion.Enemy)
			{
				State = StageState.AfterBattle;

				CoreService.Event.Publish(BattleEndEvent.Create(UnitRegion.Ally));
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

			UnitManager.DespawnEnemies();

			_room.Dispose();

			GenerateRoom();

			UnitManager.SpawnEnemies(_room.EnemySpawnArea);

			CoreService.Event.PublishImmediate(RoomChangeEndEvent.Instance);

			yield return ScreenTransition.Instance.FadeInAsync(1, this);

			State = StageState.WaitBattle;
		}

		/// <summary>
		/// Room 생성
		/// </summary>
		private void GenerateRoom()
		{
			// FIXME
			var doorCount = Random.Range(1, GameRoom.MaxDoorCount + 1);

			_room.Initialize(doorCount);
		}
	}
}