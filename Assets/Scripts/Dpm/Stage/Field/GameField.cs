using System;
using System.Collections.Generic;
using Dpm.Utility.Pool;
using UnityEngine;

namespace Dpm.Stage.Field
{
	public class GameField : MonoBehaviour, IDisposable
	{
		[SerializeField]
		private GameObject doorLayer;

		[SerializeField]
		private string[] doorIndicesByCount;

		private int[][] _doorIndicesByCount;

		[SerializeField]
		private GameObject topWallLayer;

		private class DoorHolder : IDisposable
		{
			private const float MinXOffset = -1.5f;
			private const float MaxXOffset = 2.5f;

			public Vector3 Position;
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

				_door.IsOpened = false;

				_doorObj.Deactivate();
				_doorObj = null;
				_door = null;

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

				_usingDoorHolders.Add(doorHolder);
			}
		}

		public void Dispose()
		{
			foreach (var doorHolder in _usingDoorHolders)
			{
				doorHolder.DetachDoor();
			}

			_usingDoorHolders.Clear();
		}

		public void Update()
		{
			for (int i = 0; i < _usingDoorHolders.Count; i++)
			{
				_usingDoorHolders[i].Door.IsOpened = Input.GetKey($"{i + 1}");
			}
		}
	}
}