using System;
using System.Collections.Generic;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Room;
using Dpm.Stage.Unit.State;
using Dpm.Utility.Constants;
using Dpm.Utility.Extensions;
using Dpm.Utility.Pool;
using Unity.VisualScripting;
using UnityEngine;

namespace Dpm.Stage.Unit
{
	public class UnitManager : IDisposable
	{
		private List<IUnit> _units = new();
		private Party _allyParty;
		private Party _enemyParty;

		public void Dispose()
		{
			_allyParty?.Dispose();
			_allyParty = null;

			_enemyParty?.Dispose();
			_enemyParty = null;

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
			if (TrySpawnParty("ally", spawnArea, out var party))
			{
				_allyParty = party;
			}
		}

		private bool TrySpawnParty(string partySpecName, SpawnArea spawnArea, out Party party)
		{
			UnitRegion region;

			// TODO : partySpecName으로 파티원 생성
			if (partySpecName == "ally")
			{
				region = UnitRegion.Ally;
			}
			else
			{
				party = null;

				return false;
			}

			var members = new List<Character>();

			// FIXME
			for (int i = 0; i < 4; i++)
			{
				if (TrySpawnCharacter("character_wizard", spawnArea.RandomPos, spawnArea.Direction,
					    out var member))
				{
					members.Add(member);
				}
			}

			party = new Party(region, members);

			return true;
		}

		public bool TrySpawnCharacter(string poolSpecName, Vector2 spawnPos, Direction direction, out Character character)
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