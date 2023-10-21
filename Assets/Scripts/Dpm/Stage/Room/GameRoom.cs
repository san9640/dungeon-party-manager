using System;
using System.Collections.Generic;
using Dpm.Common;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Unit;
using Dpm.Utility.Pool;
using UnityEngine;
using CustomCollider2D = Dpm.Stage.Physics.CustomCollider2D;

namespace Dpm.Stage.Room
{
	public class GameRoom : MonoBehaviour, IDisposable
	{
		[SerializeField]
		private GameObject doorLayer;

		[SerializeField]
		private string[] doorIndicesByCount;

		private int[][] _doorIndicesByCount;

		[SerializeField]
		private GameObject topWallLayer;

		[SerializeField]
		private Unit.Unit[] units;

		public Unit.Unit[] Units => units;

		private enum State
		{
			None,
			WaitDoorOpen,
			Cleared,
		}

		private State _state = State.None;

		public const int MaxDoorCount = 5;

		private class DoorHolder : IDisposable
		{
			private const float MinXOffset = -1.5f;
			private const float MaxXOffset = 2.5f;

			public readonly Vector3 Position;
			private List<GameObject> _overlappedWalls = new();

			private PooledGameObject _doorObj;
			private Door _door;

			public Door Door => _door;

			public DoorHolder(Vector3 position)
			{
				Position = position;
			}

			public void AddWhenOverlapped(GameObject wall)
			{
				var pos = wall.transform.position;

				if (pos.x >= Position.x + MinXOffset && pos.x <= Position.x + MaxXOffset)
				{
					_overlappedWalls.Add(wall);
				}
			}

			public void AttachDoor()
			{
				if (!GameObjectPool.Get("field_door").TrySpawn(Position, out _doorObj))
				{
					return;
				}

				_door = _doorObj.GetComponent<Door>();

				foreach (var wall in _overlappedWalls)
				{
					wall.SetActive(false);
				}
			}

			public void DetachDoor()
			{
				if (_doorObj == null)
				{
					return;
				}

				_door.Dispose();
				_door = null;

				_doorObj.Deactivate();
				_doorObj = null;

				foreach (var wall in _overlappedWalls)
				{
					wall.SetActive(true);
				}
			}

			public void Dispose()
			{
				_overlappedWalls?.Clear();
				_overlappedWalls = null;
			}
		}

		private DoorHolder[] _doorHolders;

		private readonly List<DoorHolder> _usingDoorHolders = new(5);

		[SerializeField]
		private SpawnArea allySpawnArea;

		public SpawnArea AllySpawnArea => allySpawnArea;

		[SerializeField]
		private SpawnArea enemySpawnArea;

		public SpawnArea EnemySpawnArea => enemySpawnArea;

		[SerializeField]
		private CustomCollider2D battleZone;

		public Bounds2D BattleZone => battleZone.bounds;

		private void Awake()
		{
			_doorIndicesByCount = new int[doorIndicesByCount.Length][];

			// string[] -> int[][] 변환
			for (var i = 0; i < doorIndicesByCount.Length; i++)
			{
				var splits = doorIndicesByCount[i].Split(',', StringSplitOptions.RemoveEmptyEntries);
				var indices = new int[splits.Length];

				for (var j = 0; j < splits.Length; j++)
				{
					var index = int.Parse(splits[j]);

					indices[j] = index;
				}

				_doorIndicesByCount[i] = indices;
			}

			_doorHolders = new DoorHolder[doorLayer.transform.childCount];

			for (var i = 0; i < _doorHolders.Length; i++)
			{
				var pos = doorLayer.transform.GetChild(i).transform.position;

				_doorHolders[i] = new DoorHolder(pos);
			}

			// 왼쪽부터 차례로 정렬
			Array.Sort(_doorHolders, (a, b) => a.Position.x.CompareTo(b.Position.x));

			for (var i = 0; i < topWallLayer.transform.childCount; i++)
			{
				var wall = topWallLayer.transform.GetChild(i);

				foreach (var doorHolder in _doorHolders)
				{
					doorHolder.AddWhenOverlapped(wall.gameObject);
				}
			}
		}

		public void Initialize(int doorCount)
		{
			foreach (var index in _doorIndicesByCount[doorCount])
			{
				var doorHolder = _doorHolders[index];

				doorHolder.AttachDoor();

				// FIXME : 이런 식으로 할당하는 것이 괜찮은 걸까?
				doorHolder.Door.Id = index;

				_usingDoorHolders.Add(doorHolder);
			}

			_state = State.None;

			CoreService.Event.Subscribe<DoorClickEvent>(OnDoorClick);
			CoreService.Event.Subscribe<BattleEndEvent>(OnBattleEnd);
		}

		public void Dispose()
		{
			CoreService.Event.Unsubscribe<DoorClickEvent>(OnDoorClick);
			CoreService.Event.Unsubscribe<BattleEndEvent>(OnBattleEnd);

			_state = State.None;

			foreach (var doorHolder in _usingDoorHolders)
			{
				doorHolder.DetachDoor();
			}

			_usingDoorHolders.Clear();
		}

		private void OnDoorClick(Core.Interface.Event e)
		{
			if (e is not DoorClickEvent doorClickEvent)
			{
				return;
			}

			// 스크린 페이드 중에는 동작하지 않도록 함
			if (_state == State.WaitDoorOpen && !ScreenTransition.Instance.Enabled)
			{
				if (doorClickEvent.DoorIndex >= 0 && doorClickEvent.DoorIndex < _doorHolders.Length)
				{
					_state = State.Cleared;

					_doorHolders[doorClickEvent.DoorIndex].Door.IsOpened = true;

					CoreService.Event.Publish(RoomClearedEvent.Instance);
				}
			}
		}

		private void OnBattleEnd(Core.Interface.Event e)
		{
			if (e is not BattleEndEvent bee)
			{
				return;
			}

			if (bee.WonPartyRegion == UnitRegion.Ally)
			{
				_state = State.WaitDoorOpen;
			}
		}
	}
}