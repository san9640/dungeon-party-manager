﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Room;
using Dpm.Stage.Spec;
using Dpm.Utility.Constants;
using Dpm.Utility.Pool;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

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
		
		private float _lastRoundEnemyTotalStats = 0;

		private int _unitCount = 4;
		
		List<(int, int)> spawnPositions = new List<(int, int)>
		{
			(0, 0), (2, 2), (4, 0), (2, -2),
		};

		private int _totalClearUnits = 0;
		public int TotalClearUnits => _totalClearUnits;
		
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
			List<CharacterSpec> selectedCharacters = new List<CharacterSpec>();
			var members = new List<Character>();
			var allCharacters = partySpec.spawnInfos.Select(info => SpecUtility.GetSpec<CharacterSpec>(info.characterSpecName)).ToList();
			float totalStats = 0;
			int maxAttempts = 1000;
			int attempt = 0;
			var sortedCharacters = allCharacters.OrderBy(character => character.baseHp + character.baseDamageFactor + character.baseAttackSpeed*10).ToList();
			
			do
			{
				if (_unitCount == 9)
				{
					selectedCharacters = sortedCharacters;
					break;
				}
				selectedCharacters.Clear();
				// maxAttempts를 초과하면 유닛 하나 추가
				if (attempt == maxAttempts -1)
				{
					_unitCount++;
					for (int i = 4; i <= 8; i += 2)
					{
						for (int j = -2; j <= 2; j += 2)
						{
							var newPos = (i, j);
							// 기존의 좌표와 중복되지 않는 경우에만 추가
							if (!spawnPositions.Contains(newPos))  
							{
								spawnPositions.Add(newPos);
								break;
							}
						}

						if (spawnPositions.Count == _unitCount)
							break;
					}
					attempt = 0;
				}
						
				for (int i = 0; i < _unitCount; i++)
				{
					double randomValue = (double)Random.Range(1, 9)/ sortedCharacters.Count; 

					for (int j = 0; j < sortedCharacters.Count; j++)
					{
						// 확률적 가중치. 스탯이 낮은 캐릭터일수록 높은 확률로 선택
						double probability = (double)(j + 1) / sortedCharacters.Count;


						if (randomValue <= probability)
						{
							selectedCharacters.Add(sortedCharacters[j]);
							break;
						}
					}
				}

				totalStats = selectedCharacters.Sum(character => character.baseHp + character.baseDamageFactor + character.baseAttackSpeed*10);

				attempt++;
			}
			while (totalStats <= _lastRoundEnemyTotalStats && attempt < maxAttempts);

			// 선택된 캐릭터와 좌표를 이용하여 유닛 생성
			for (int i = 0; i < selectedCharacters.Count; i++)
			{
				if (TrySpawnCharacter(selectedCharacters[i].Name,
					    spawnArea[spawnPositions[i].Item1, spawnPositions[i].Item2], spawnArea.Direction,
					    out var member))
				{
					members.Add(member);
				}
			}
			_lastRoundEnemyTotalStats = totalStats;
			party = new Party(partySpec.region, members);
			// 처치 유닛수 누적
			_totalClearUnits += members.Count;
			
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