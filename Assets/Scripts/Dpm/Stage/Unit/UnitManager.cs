using System;
using System.Collections.Generic;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Room;
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

		public void Dispose()
		{
			DespawnParty(ref _allyParty);
			DespawnParty(ref _enemyParty);

			// 모든 유닛 해제
			foreach (var unit in _units)
			{
				UnregisterUnit(unit);
			}

			_units?.Clear();
			_units = null;
		}

		public void SpawnAllies(SpawnArea spawnArea)
		{
			if (_allyParty == null && TrySpawnParty("ally", spawnArea, out var party))
			{
				_allyParty = party;
			}
		}

		public void SpawnEnemies(SpawnArea spawnArea)
		{
			if (_enemyParty == null && TrySpawnParty("enemy", spawnArea, out var party))
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

			foreach (var enemy in _enemyParty.Members)
			{
				DespawnCharacter(enemy);
			}

			party.Dispose();
			party = null;
		}

		private bool TrySpawnParty(string partySpecName, SpawnArea spawnArea, out Party party)
		{
			UnitRegion region;

			PartySpec partySpec;

			// TODO : partySpecName으로 파티원 생성
			if (partySpecName == "ally")
			{
				region = UnitRegion.Ally;

				// FIXME : 임시
				partySpec = new PartySpec
				{
					poolSpecNames = new []
					{
						"character_wizard",
						"character_wizard",
						"character_wizard",
						"character_wizard",
					}
				};
			}
			else if (partySpecName == "enemy")
			{
				region = UnitRegion.Enemy;

				// FIXME : 임시
				partySpec = new PartySpec
				{
					poolSpecNames = new[]
					{
						"character_orc_warrior",
						"character_orc_warrior",
						"character_orc_warrior",
						"character_orc_warrior",
					}
				};
			}
			else
			{
				party = null;

				return false;
			}

			var members = new List<Character>();

			// FIXME
			foreach (var prefabSpecName in partySpec.poolSpecNames)
			{
				if (TrySpawnCharacter(prefabSpecName, spawnArea.RandomPos, spawnArea.Direction,
					    out var member))
				{
					members.Add(member);
				}
			}

			party = new Party(region, members);

			return true;
		}

		private bool TrySpawnCharacter(string poolSpecName, Vector2 spawnPos, Direction direction, out Character character)
		{
			var pool = GameObjectPool.Get(poolSpecName);

			if (pool.IsUnityNull() || !pool.TrySpawn(Vector3.zero, out var go))
			{
				character = null;
				return false;
			}

			character = go.GetComponent<Character>();
			character.Direction = direction;
			character.Position = spawnPos;

			RegisterUnit(character);

			return true;
		}

		public void DespawnCharacter(Character character)
		{
			UnregisterUnit(character);

			var pooledGo = character.GetComponent<PooledGameObject>();

			pooledGo.Deactivate();
		}

		public void RegisterUnit(IUnit unit)
		{
			_units.Add(unit);

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