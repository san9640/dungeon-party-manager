using System;
using System.Collections;
using System.Collections.Generic;
using Core.Interface;
using Dpm.Common;
using Dpm.Common.Event;
using Dpm.CoreAdapter;
using Dpm.Stage.Buff;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Room;
using Dpm.Stage.Spec;
using Dpm.Stage.UI;
using Dpm.Stage.UI.Event;
using Dpm.Stage.Unit;
using Dpm.Utility.Pool;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
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

		public Bounds2D BattleZone => _room.BattleZone;

		public StageState State { get; private set; } = StageState.None;

		public StagePhysicsManager PhysicsManager { get; private set; }

		public UnitManager UnitManager { get; private set; }

		private StageUIManager _stageUIManager;

		public ProjectileManager ProjectileManager { get; private set; }

		private readonly List<BuffSpec> _currentDoorBuffs = new();

		private static readonly string[] BuffNames =
		{
			"room_buff_max_hp",
			"room_buff_attack_speed",
			"room_buff_damage",
		};

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
			if (e is not RoomClearedEvent rce)
			{
				return;
			}

			var buff = _currentDoorBuffs[rce.DoorIndex];

			CoreService.Coroutine.StartCoroutine(MoveRoomAsync(buff));
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

				// GenerateDoorBuffs();

				CoreService.Event.Publish(BattleEndEvent.Create(UnitRegion.Ally));
			}
		}

		/// <summary>
		/// 클리어 이후 방을 이동하는 코루틴
		/// </summary>
		private IEnumerator MoveRoomAsync(BuffSpec buff)
		{
			State = StageState.Loading;

			yield return ScreenTransition.Instance.FadeOutAsync(1, this);

			foreach (var ally in UnitManager.AllyParty.Members)
			{
				Core.Interface.Event buffEvent = buff.type switch
				{
					BuffType.AttackSpeed => AddAttackSpeedBuffEvent.Create(null, buff.value),
					BuffType.Damage => AddDamageBuffEvent.Create(null, buff.value),
					BuffType.MaxHp => AddMaxHpBuffEvent.Create(null, buff.IntValue),
					_ => null
				};

				if (buffEvent != null)
				{
					CoreService.Event.SendImmediate(ally, buffEvent);
				}

				var healEvent = HealEvent.Create(ally, ally.MaxHp);

				CoreService.Event.SendImmediate(ally, healEvent);
			}

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
			var doorCount = 1 + Random.Range(0, Math.Min(BuffNames.Length, GameRoom.MaxDoorCount));

			_room.Initialize(doorCount);

			GenerateDoorBuffs();
		}

		private void GenerateDoorBuffs()
		{
			_currentDoorBuffs.Clear();

			var pooledList = PooledList<int>.Get();

			for (var i = 0; i < BuffNames.Length; i++)
			{
				pooledList.Add(i);
			}

			for (var i = 0; i < _room.DoorCount; i++)
			{
				var r = Random.Range(0, pooledList.Count);

				var buffIndex = pooledList[r];
				pooledList.RemoveAt(r);

				var spedName = BuffNames[buffIndex];
				var buff = SpecUtility.GetSpec<BuffSpec>(spedName);

				var text = buff.type switch
				{
					BuffType.MaxHp => $"HP\n+{buff.IntValue}",
					BuffType.Damage => $"ATK\nx{(buff.value + 1):N2}",
					BuffType.AttackSpeed => $"ASD\nx{(buff.value + 1):N2}",
					_ => string.Empty
				};

				_room.SetBuffText(i, text);

				_currentDoorBuffs.Add(buff);
			}

			pooledList.Dispose();
		}
	}
}