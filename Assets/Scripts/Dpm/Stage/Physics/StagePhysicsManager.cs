using System;
using System.Collections.Generic;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Unit;
using Dpm.Utility.Constants;
using Dpm.Utility.Extensions;
using UnityEngine;

namespace Dpm.Stage.Physics
{
	public class StagePhysicsManager : IDisposable
	{
		/// <summary>
		/// FIXME : 현재는 "파티션"이라는 용어와 무관하게 간단하게 모든 유닛을 비교하지만, 성능 상에 이슈가 생긴다면 파티션 분할을 실제로 실행해야 할 것
		/// </summary>
		private readonly List<ICustomCollider> _colliders = new();

		public static StagePhysicsManager Instance => Game.Instance.Stage.PhysicsManager;

		public StagePhysicsManager()
		{
			CoreService.Event.Subscribe<AddToPartitionEvent>(OnAddToPartition);
			CoreService.Event.Subscribe<RemoveFromPartitionEvent>(OnRemoveFromPartition);
		}

		public void Dispose()
		{
			CoreService.Event.Unsubscribe<AddToPartitionEvent>(OnAddToPartition);
			CoreService.Event.Unsubscribe<RemoveFromPartitionEvent>(OnRemoveFromPartition);
		}

		private void OnAddToPartition(Core.Interface.Event e)
		{
			if (e is not AddToPartitionEvent ape)
			{
				return;
			}

			_colliders.Remove(ape.Collider);
			_colliders.Add(ape.Collider);
		}

		private void OnRemoveFromPartition(Core.Interface.Event e)
		{
			if (e is not RemoveFromPartitionEvent rpe)
			{
				return;
			}

			_colliders.Remove(rpe.Collider);
		}

		public bool SimulateMove(Bounds2D bounds, Vector2 dir, float magnitude,
			UnitRegion canCollideUnitRegions, out Vector2 endPos)
		{
			var moveDiff = dir * magnitude;

			// 너무 조금 움직이는 것은 계산하지 않음
			if (moveDiff.IsAlmostZero())
			{
				endPos = bounds.center;
				return false;
			}

			var xCrashed = SimulateMoveAxis(bounds, moveDiff.x, canCollideUnitRegions, true, out endPos);

			bounds.center = endPos;

			var zCrashed = SimulateMoveAxis(bounds, moveDiff.y, canCollideUnitRegions, false, out endPos);

			return xCrashed || zCrashed;
		}

		public bool MoveUnit(IUnit unit, Vector2 dir, float magnitude)
		{
			// FIXME : 투사체 등의 옵션에 따라 다르게 동작하게 만들기
			var canCollideUnitRegions = UnitRegion.All;

			var crashed = SimulateMove(unit.Bounds, dir, magnitude,
				canCollideUnitRegions, out var endPos);

			unit.Position = endPos;

			return crashed;
		}

		private bool SimulateMoveAxis(Bounds2D bounds, float diff,
			UnitRegion canCollideUnitRegions, bool isXAxis, out Vector2 endPos)
		{
			var axisIndex = isXAxis ? 0 : 1;
			var anotherAxisIndex = 1 - axisIndex;
			var anotherAxisMin = bounds.Min[anotherAxisIndex];
			var anotherAxisMax = bounds.Max[anotherAxisIndex];
			float newCenterAxis;

			if (diff == 0)
			{
				endPos = bounds.center;

				return false;
			}

			if (diff > 0)
			{
				var boundsMax = bounds.Max[axisIndex];
				var targetBoundsMax = boundsMax + diff;
				var resultBoundsMax = targetBoundsMax;

				foreach (var other in _colliders)
				{
					// 충돌할 타입과 다르면 패스
					if (other is IUnit otherUnit &&
					    (otherUnit.Region & canCollideUnitRegions) == UnitRegion.None)
					{
						continue;
					}

					if (anotherAxisMax > other.Bounds.Min[anotherAxisIndex] &&
					    anotherAxisMin < other.Bounds.Max[anotherAxisIndex])
					{
						var otherBoundsMin = other.Bounds.Min[axisIndex];

						// 충돌이 없었다가 발생했을 때
						if (boundsMax < otherBoundsMin &&
						    targetBoundsMax + GameConstants.Epsilon >= otherBoundsMin)
						{
							var newResultBoundMax = otherBoundsMin - GameConstants.Epsilon;

							if (newResultBoundMax < resultBoundsMax)
							{
								resultBoundsMax = newResultBoundMax;
							}
						}
					}
				}

				newCenterAxis = resultBoundsMax - bounds.extents[axisIndex];
			}
			else
			{
				var boundsMin = bounds.Min[axisIndex];

				var targetBoundsMin = boundsMin + diff;
				var resultBoundsMin = targetBoundsMin;

				foreach (var other in _colliders)
				{
					// 충돌할 타입과 다르면 패스
					if (other is IUnit otherUnit &&
					    (otherUnit.Region & canCollideUnitRegions) == UnitRegion.None)
					{
						continue;
					}

					if (anotherAxisMax > other.Bounds.Min[anotherAxisIndex] &&
					    anotherAxisMin < other.Bounds.Max[anotherAxisIndex])
					{
						var otherBoundsMax = other.Bounds.Max[axisIndex];

						// 충돌이 없었다가 발생했을 때
						if (boundsMin > otherBoundsMax &&
						    targetBoundsMin - GameConstants.Epsilon <= otherBoundsMax)
						{
							var newResultBoundMin = otherBoundsMax + GameConstants.Epsilon;

							if (newResultBoundMin > resultBoundsMin)
							{
								resultBoundsMin = newResultBoundMin;
							}
						}
					}
				}

				newCenterAxis = resultBoundsMin + bounds.extents[axisIndex];
			}

			endPos = isXAxis ?
				new Vector2(newCenterAxis, bounds.center[anotherAxisIndex]) :
				new Vector2(bounds.center[anotherAxisIndex], newCenterAxis);

			return false;
		}
	}
}