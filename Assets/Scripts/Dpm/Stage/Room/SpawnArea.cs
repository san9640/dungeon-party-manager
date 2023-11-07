using Dpm.Stage.Physics;
using Dpm.Stage.Unit;
using Dpm.Utility;
using Dpm.Utility.Constants;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dpm.Stage.Room
{
	public class SpawnArea : MonoBehaviour
	{
		public static int SpawnPosSize = 2;

		[SerializeField]
		private Vector2 minOffset;

		[SerializeField]
		private Vector2 maxOffset;

		[SerializeField]
		private Direction direction;

		public Vector2 Min
		{
			get
			{
				var pivot = new Vector2(transform.position.x, transform.position.y);

				return pivot + minOffset;
			}
		}

		public Vector2 Max
		{
			get
			{
				var pivot = new Vector2(transform.position.x, transform.position.y);

				return pivot + maxOffset;
			}
		}

		public Direction Direction => direction;

		public Vector2 this[int xIndex, int yIndex]
		{
			get
			{
				var offset = new Vector2(xIndex, yIndex) * SpawnPosSize;

				if (Direction == Direction.Right)
				{
					offset.x = -offset.x;
				}

				return offset + Pivot;
			}
		}

		private Vector2 Pivot
		{
			get
			{
				if (_pivot.HasValue)
				{
					return _pivot.Value;
				}

				var min = transform.position + new Vector3(minOffset.x, minOffset.y, 0);
				var max = transform.position + new Vector3(maxOffset.x, maxOffset.y, 0);
				var center = (min + max) / 2;

				var pivot = Direction == Direction.Left ?
					new Vector2(min.x + SpawnPosSize * 0.5f, center.y) :
					new Vector2(max.x - SpawnPosSize * 0.5f, center.y);

				_pivot = pivot;

				return pivot;
			}
		}

		private Vector2? _pivot;

		public Vector2 GetClosestSpawnPos(Party party, Character character, Vector2 position)
		{
			var min = transform.position + new Vector3(minOffset.x, minOffset.y, 0);
			var max = transform.position + new Vector3(maxOffset.x, maxOffset.y, 0);
			var center = (min + max) / 2;
			var size = max - min;

			var maxXCount = Mathf.FloorToInt((size.x - SpawnPosSize) / SpawnPosSize);
			var maxYHalfCount = Mathf.FloorToInt((size.y - SpawnPosSize) * 0.5f / SpawnPosSize);

			float? minSqrDist = null;
			var closest = Vector2.zero;

			for (var x = 0; x <= maxXCount; x++)
			{
				for (var y = -maxYHalfCount; y <= maxYHalfCount; y++)
				{
					var spawnPos = this[x, y];

					var sqrDist = (position - spawnPos).sqrMagnitude;

					if (!minSqrDist.HasValue || minSqrDist.Value > sqrDist)
					{
						var collapse = false;

						foreach (var member in party.Members)
						{
							if (member != character && member.Bounds.Contains(spawnPos))
							{
								collapse = true;
								break;
							}
						}

						if (collapse)
						{
							continue;
						}

						minSqrDist = sqrDist;
						closest = spawnPos;
					}
				}
			}

			return closest;
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (Application.isPlaying)
			{
				return;
			}

			var min = transform.position + new Vector3(minOffset.x, minOffset.y, 0);
			var max = transform.position + new Vector3(maxOffset.x, maxOffset.y, 0);
			var center = (min + max) / 2;
			var size = max - min;

			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(center, size);

			var pivot = Direction == Direction.Left ?
				new Vector3(min.x + SpawnPosSize * 0.5f, center.y, 0) :
				new Vector3(max.x - SpawnPosSize * 0.5f, center.y, 0);

			var maxXCount = Mathf.FloorToInt((size.x - SpawnPosSize) / SpawnPosSize);
			var maxYHalfCount = Mathf.FloorToInt((size.y - SpawnPosSize) * 0.5f / SpawnPosSize);

			for (var x = 0; x <= maxXCount; x++)
			{
				for (var y = -maxYHalfCount; y <= maxYHalfCount; y++)
				{
					var offset = new Vector3(x, y, 0) * SpawnPosSize;

					if (Direction == Direction.Right)
					{
						offset.x = -offset.x;
					}

					var drawPos = pivot + offset;

					Gizmos.color = (x == 0 && y == 0) ? Color.green : Color.white;
					Gizmos.DrawWireCube(drawPos, new Vector3(1, 1, 0));
				}
			}
		}
#endif
	}
}