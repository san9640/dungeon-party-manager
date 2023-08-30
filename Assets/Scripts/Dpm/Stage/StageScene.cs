﻿using System.Collections;
using System.Collections.Generic;
using Core.Interface;
using Dpm.Common;
using Dpm.Common.Event;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Room;
using Dpm.Stage.Unit;
using Dpm.Utility;
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

	public class StageScene : IScene
	{
		private const string SceneName = "Stage";

		private GameRoom _field;

		public StageState State { get; private set; } = StageState.None;

		public Party AllyParty { get; private set; }

		public Party EnemyParty { get; private set; }

		public StagePartitionManager PartitionManager { get; private set; }

		private List<IUnit> _units = new();

		public IEnumerator LoadAsync()
		{
			yield return UnityScene.LoadSceneAsync(SceneName);

			PartitionManager = new StagePartitionManager();

			if (!CoreService.Asset.TryGet<GameObject>("field", out var prefab))
			{
				yield break;
			}

			var fieldGo = Object.Instantiate(prefab);

			_field = fieldGo.GetComponent<Room.GameRoom>();

			GenerateField();

			var allies = new List<Character>();

			// FIXME
			for (int i = 0; i < 4; i++)
			{
				var ally = GenerateCharacter("character_wizard");

				ally.Position = _field.AllySpawnArea.RandomPos;
				ally.Animator.LookDirection = _field.AllySpawnArea.Direction;

				allies.Add(ally);
			}

			AllyParty = new Party(UnitRegion.Ally, allies);
		}

		public void Enter()
		{
			foreach (var unit in _units)
			{
				unit.EnterField();
			}

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

			foreach (var unit in _units)
			{
				unit.ExitField();
			}

			AllyParty?.Dispose();
			AllyParty = null;

			EnemyParty?.Dispose();
			EnemyParty = null;

			if (!_field.IsDestroyedOrNull())
			{
				// TODO : 그리드 옮겨갈 때마다 리셋해주는 것으로 변경
				_field.Dispose();

				Object.Destroy(_field.gameObject);
			}

			_field = null;

			PartitionManager?.Dispose();
			PartitionManager = null;

			_units.Clear();
			_units = null;
		}

		private void OnExitStage(Core.Interface.Event e)
		{
			Game.Instance.MoveToMainMenu();
		}

		private void OnFieldCleared(Core.Interface.Event e)
		{
			CoreService.Coroutine.StartCoroutine(MoveFieldAsync());
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

		private IEnumerator MoveFieldAsync()
		{
			State = StageState.Loading;

			yield return ScreenTransition.Instance.FadeOutAsync(1, this);

			foreach (var unit in _units)
			{
				unit.ExitField();
			}

			_field.Dispose();

			GenerateField();

			// FIXME : Field에 들어와있는지 확인
			foreach (var unit in _units)
			{
				unit.EnterField();
			}

			yield return ScreenTransition.Instance.FadeInAsync(1, this);

			State = StageState.WaitBattle;
		}

		private void GenerateField()
		{
			// FIXME
			var doorCount = Random.Range(1, Room.GameRoom.MaxDoorCount + 1);
			_field.Initialize(doorCount);
		}

		/// <summary>
		/// 캐릭터 생성
		/// FIXME : Generator가 많아지면 분리 필요
		/// </summary>
		/// <param name="specName"></param>
		/// <returns></returns>
		private Character GenerateCharacter(string specName)
		{
			if (!CoreService.Asset.TryGet<GameObject>(specName, out var prefab))
			{
				return null;
			}

			var go = Object.Instantiate(prefab);
			var character = go.GetComponent<Character>();

			_units.Add(character);

			return character;
		}
	}
}