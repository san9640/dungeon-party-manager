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

		public MoveResult Move(ICustomCollider collider, Vector2 dir, float magnitude)
		{
			var result = SimulateMove(collider, dir, magnitude);

			collider.Position = result.endPos;

			if (result.crasher != null)
			{
				CoreService.Event.Send(result.crasher, CrashedEvent.Create(collider));
				CoreService.Event.Send(collider, CrashedEvent.Create(result.crasher));
			}

			return result;
		}

		public MoveResult SimulateMove(ICustomCollider collider, Vector2 dir, float magnitude)
		{
			var moveDiff = dir * magnitude;
			var bounds = collider.Bounds;

			var xMoveResult = SimulateMoveAxis(collider, bounds.center, moveDiff.x, true);
			var yMoveResult = SimulateMoveAxis(collider, xMoveResult.endPos, moveDiff.y, false);

			var result = new MoveResult
			{
				// 시뮬레이션 결과물에 좀 더 가까운 y 충돌 대상으로 넣어줌
				// FIXME : 충돌 결과를 한 개만 뱉게 되어있음.
				crasher = yMoveResult.crasher ?? xMoveResult.crasher,
				endPos = yMoveResult.endPos,
			};

			return result;
		}

		private MoveResult SimulateMoveAxis(ICustomCollider collider, Vector2 position, float diff, bool isXAxis)
		{
			var bounds = collider.Bounds;
			bounds.center = position;

			var axisIndex = isXAxis ? 0 : 1;
			var anotherAxisIndex = 1 - axisIndex;
			var anotherAxisMin = bounds.Min[anotherAxisIndex];
			var anotherAxisMax = bounds.Max[anotherAxisIndex];
			float newCenterAxis;

			var result = new MoveResult();

			if (diff == 0)
			{
				result.endPos = bounds.center;

				return result;
			}

			if (diff > 0)
			{
				var boundsMax = bounds.Max[axisIndex];
				var targetBoundsMax = boundsMax + diff;
				var resultBoundsMax = targetBoundsMax;

				foreach (var other in _colliders)
				{
					if (!(collider.OnSimulateCrash(other) || other.OnSimulateCrash(collider)))
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
								result.crasher = other;
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
					if (!(collider.OnSimulateCrash(other) || other.OnSimulateCrash(collider)))
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
								result.crasher = other;
							}
						}
					}
				}

				newCenterAxis = resultBoundsMin + bounds.extents[axisIndex];
			}

			result.endPos = isXAxis ?
				new Vector2(newCenterAxis, bounds.center[anotherAxisIndex]) :
				new Vector2(bounds.center[anotherAxisIndex], newCenterAxis);

			return result;
		}
	}
}