using System;
using System.Collections.Generic;
using Dpm.Stage.Room;
using Dpm.Utility.Constants;
using Dpm.Utility.Extensions;
using Dpm.Utility.Pool;
using UnityEngine;

namespace Dpm.Stage.Unit
{
	public class UnitManager : IDisposable
	{
		private List<IUnit> _units = new();
		private Party _allyParty;
		private Party _enemyParty;

		public UnitManager()
		{
		}

		public void Dispose()
		{
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

			if (pool == null || !pool.TrySpawn(Vector3.zero, out var go))
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
		}

		public void UnregisterUnit(IUnit unit)
		{
			_units.Remove(unit);
		}

		// FIXME : Field 진입을 직접 호출하지 않고, 이벤트로 관리하도록 변경해야 함
		public void EnterFieldAll()
		{
			foreach (var unit in _units)
			{
				unit.EnterField();
			}
		}

		public void ExitFieldAll()
		{
			foreach (var unit in _units)
			{
				unit.ExitField();
			}
		}
	}
}