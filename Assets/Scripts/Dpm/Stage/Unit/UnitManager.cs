using System;
using System.Collections.Generic;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Room;
using Dpm.Stage.Spec;
using Dpm.Utility.Constants;
using Dpm.Utility.Pool;
using Unity.VisualScripting;
using UnityEngine;

namespace Dpm.Stage.Unit
{
	public class UnitManager : IDisposable
	{
		public static UnitManager Instance => StageScene.Instance.UnitManager;

		private List<IUnit> _units = new();

		private Party _allyParty;

		public Party AllyParty => _allyParty;

		private Party _enemyParty;

		public Party EnemyParty => _enemyParty;

		private int _nextUnitId = 1;

		private readonly Dictionary<Character, PooledGameObject> _statBars = new();

		public void Dispose()
		{
			DespawnParty(ref _allyParty);
			DespawnParty(ref _enemyParty);

			// 모든 유닛 해제
			while (_units.Count > 0)
			{
				UnregisterUnit(_units[0]);
			}

			_units?.Clear();
			_units = null;

			_statBars.Clear();
		}

		public Party GetMyParty(UnitRegion region)
		{
			return region switch
			{
				UnitRegion.Ally => _allyParty,
				UnitRegion.Enemy => _enemyParty,
				_ => null
			};
		}

		public Party GetOppositeParty(UnitRegion region)
		{
			return region switch
			{
				UnitRegion.Ally => _enemyParty,
				UnitRegion.Enemy => _allyParty,
				_ => null
			};
		}

		public void SpawnAllies(string partySpecName, SpawnArea spawnArea)
		{
			if (_allyParty == null && TrySpawnParty(partySpecName, spawnArea, out var party))
			{
				_allyParty = party;
			}
		}

		public void SpawnEnemies(string partySpecName, SpawnArea spawnArea)
		{
			if (_enemyParty == null && TrySpawnRandomParty(partySpecName, spawnArea, out var party))
			{
				_enemyParty = party;
			}
		}

		public void DespawnEnemies()
		{
			DespawnParty(ref _enemyParty);
		}

		private void DespawnParty(ref Party party)
		{
			if (party == null)
			{
				return;
			}

			foreach (var enemy in party.Members)
			{
				DespawnCharacter(enemy);
			}

			party.Dispose();
			party = null;
		}

		private bool TrySpawnParty(string partySpecName, SpawnArea spawnArea, out Party party)
		{
			var partySpec = SpecUtility.GetSpec<PartySpec>(partySpecName);
			var members = new List<Character>();

			foreach (var spawnInfo in partySpec.spawnInfos)
			{
				if (TrySpawnCharacter(spawnInfo.characterSpecName,
					    spawnArea[spawnInfo.spawnIndex.x, spawnInfo.spawnIndex.y], spawnArea.Direction,
					    out var member))
				{
					members.Add(member);
				}
			}

			party = new Party(partySpec.region, members);

			return true;
		}
		
		private bool TrySpawnRandomParty(string partySpecName, SpawnArea spawnArea, out Party party)
		{
			var partySpec = SpecUtility.GetSpec<PartySpec>(partySpecName);
			var members = new List<Character>();
			
			int[,] arraySpawnPos = new int[4,2]
			{
				{ 0, 0 },
				{ 2, 2 },
				{ 4, 0 },
				{ 2, -2 }
			};

			int count = 0;
			
			while (count < 4)
			{
				int unitCount = UnityEngine.Random.Range(0, partySpec.spawnInfos.Length);

				if (TrySpawnCharacter(partySpec.spawnInfos[unitCount].characterSpecName,
					    spawnArea[arraySpawnPos[count,0],arraySpawnPos[count,1]], spawnArea.Direction,
					    out var member))
				{
					members.Add(member);
					count++;
				}
			}

			party = new Party(partySpec.region, members);

			return true;
		}

		private bool TrySpawnCharacter(string characterSpecName, Vector2 spawnPos, Direction direction, out Character character)
		{
			var characterSpec = SpecUtility.GetSpec<CharacterSpec>(characterSpecName);

			var pool = GameObjectPool.Get(characterSpec.poolSpecName);

			if (pool.IsUnityNull() || !pool.TrySpawn(Vector3.zero, out var go))
			{
				character = null;
				return false;
			}

			character = go.GetComponent<Character>();
			character.Direction = direction;
			character.Position = spawnPos;

			character.Init(characterSpec);

			RegisterUnit(character);

			character.Name = $"{characterSpecName}_{character.Id}";

			var statBarPool = GameObjectPool.Get("field_ui_stat_bar");

			if (!statBarPool.IsUnityNull() && statBarPool.TrySpawn(Vector3.zero, out var statBarGo))
			{
				var statBar = statBarGo.GetComponent<FieldUIStatBar>();

				statBar.transform.parent = character.transform;
				statBar.transform.localPosition = new Vector3(0, -character.Bounds.extents.y, 0);

				statBar.Init(character);

				_statBars.Add(character, statBarGo);
			}

			return true;
		}

		public void DespawnCharacter(Character character)
		{
			UnregisterUnit(character);

			character.Dispose();

			if (_statBars.TryGetValue(character, out var statBarGo))
			{
				var statBar = statBarGo.GetComponent<FieldUIStatBar>();

				statBar.Dispose();

				statBarGo.Deactivate();

				_statBars.Remove(character);
			}

			var pooledGo = character.GetComponent<PooledGameObject>();

			pooledGo.Deactivate();
		}

		public void RegisterUnit(IUnit unit)
		{
			_units.Add(unit);

			unit.Id = _nextUnitId++;

			CoreService.Event.SendImmediate(unit, UnitRegisteredEvent.Instance);
		}

		public void UnregisterUnit(IUnit unit)
		{
			// State 비워줌
			CoreService.Event.SendImmediate(unit, UnitUnregisteredEvent.Instance);

			_units.Remove(unit);
		}
	}
}