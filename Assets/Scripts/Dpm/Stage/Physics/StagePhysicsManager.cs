using System;
using System.Collections.Generic;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Unit;
using Dpm.Utility.Constants;
using Dpm.Utility.Extensions;
using Dpm.Utility.Pool;
using UnityEngine;

namespace Dpm.Stage.Physics
{
	[Flags]
	public enum CrashOption
	{
		None = 0,
		CrashOnOverlap = 1 << 0,
	}

	public class StagePhysicsManager : IDisposable
	{
		/// <summary>
		/// FIXME : 현재는 "파티션"이라는 용어와 무관하게 간단하게 모든 유닛을 비교하지만, 성능 상에 이슈가 생긴다면 파티션 분할을 실제로 실행해야 할 것
		/// </summary>
		private readonly List<ICustomCollider> _colliders = new();

		public static StagePhysicsManager Instance => Game.Instance.Stage.PhysicsManager;

		private readonly Queue<(Vector2Int node, Vector2Int firstNode, int depth)> _bfsQueue = new();

		private readonly HashSet<int> _visitedIds = new();

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

			if (!_colliders.Contains(ape.Collider))
			{
				_colliders.Add(ape.Collider);
			}
		}

		private void OnRemoveFromPartition(Core.Interface.Event e)
		{
			if (e is not RemoveFromPartitionEvent rpe)
			{
				return;
			}

			_colliders.Remove(rpe.Collider);
		}

		public MoveResult Move(ICustomCollider collider, Vector2 dir, float magnitude, CrashOption option = CrashOption.None)
		{
			var result = SimulateMove(collider, dir, magnitude, option);

			collider.Position = result.endPos;

			if (result.crasher != null)
			{
				CoreService.Event.Send(result.crasher, CrashedEvent.Create(collider));
				CoreService.Event.Send(collider, CrashedEvent.Create(result.crasher));
			}

			return result;
		}

		public MoveResult SimulateMove(ICustomCollider collider, Vector2 dir, float magnitude, CrashOption option = CrashOption.None)
		{
			var crashOnOverlap = (option & CrashOption.CrashOnOverlap) != CrashOption.None;

			if (crashOnOverlap)
			{
				ICustomCollider crasher = null;

				foreach (var other in _colliders)
				{
					if (!(collider.OnSimulateCrash(other) || other.OnSimulateCrash(collider)))
					{
						continue;
					}

					if (PhysicsUtility.IsOverlapped(collider, other))
					{
						crasher = other;
						break;
					}
				}

				if (crasher != null)
				{
					return new MoveResult
					{
						crasher = crasher,
						endPos = collider.Position,
					};
				}
			}

			var moveDiff = dir * magnitude;
			var bounds = collider.Bounds;
			var xMoveResult = SimulateMoveAxis(collider, bounds.center, moveDiff.x, true);
			var yMoveResult = SimulateMoveAxis(collider, xMoveResult.endPos, moveDiff.y, false);

			return new MoveResult
			{
				// 시뮬레이션 결과물에 좀 더 가까운 y 충돌 대상으로 넣어줌
				// FIXME : 충돌 결과를 한 개만 뱉게 되어있음.
				crasher = yMoveResult.crasher ?? xMoveResult.crasher,
				endPos = yMoveResult.endPos,
			};
		}

		private MoveResult SimulateMoveAxis(ICustomCollider collider, Vector2 position, float diff, bool isXAxis)
		{
			var bounds = collider.Bounds;
			bounds.center = position;

			var result = new MoveResult();

			if (diff == 0)
			{
				result.endPos = bounds.center;

				return result;
			}

			var axisIndex = isXAxis ? 0 : 1;
			var anotherAxisIndex = 1 - axisIndex;
			var anotherAxisMin = bounds.Min[anotherAxisIndex];
			var anotherAxisMax = bounds.Max[anotherAxisIndex];
			float newCenterAxis;

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

		private static Vector2Int[] _bfsDirections =
		{
			Vector2Int.right,
			Vector2Int.left,
			Vector2Int.down,
			Vector2Int.up,
		};

		public bool TryFindPath(Character character, Vector2 targetPos, out Vector2 result)
		{
			var nodeOffset = character.Bounds.Size;
			var found = false;
			result = Vector2.zero;

			_bfsQueue.Enqueue((Vector2Int.zero, Vector2Int.zero, 0));

			var targetBounds = new Bounds2D(targetPos, nodeOffset);

			while (_bfsQueue.TryDequeue(out var currentNodeInfo))
			{
				var currentNode = currentNodeInfo.node;
				var firstNode = currentNodeInfo.depth == 1 ? currentNodeInfo.firstNode : currentNode;
				var currentPos = character.Position +
				                 currentNode.x * nodeOffset.x * Vector2.right +
				                 currentNode.y * nodeOffset.y * Vector2.up;

				// 적당히 거리가 가까워졌으면 탈출. 어차피 자주 불릴 것이라 대충 방향만 찾으면 됨
				if (targetBounds.Min.x <= currentPos.x && currentPos.x < targetBounds.Max.x &&
				    targetBounds.Min.y <= currentPos.y && currentPos.y < targetBounds.Max.y)
				{
					result = firstNode;
					found = true;
					break;
				}

				for (var i = 0; i < 4; i++)
				{
					var curDir = _bfsDirections[i];
					var nextNode = currentNode + curDir;

					if (_visitedIds.Contains(GetNodeId(nextNode)))
					{
						continue;
					}

					var nextPos = currentPos + curDir.x * nodeOffset.x * Vector2.right +
					              curDir.y * nodeOffset.y * Vector2.up;

					var crashed = false;

					foreach (var other in _colliders)
					{
						if (other is Character otherCharacter)
						{
							if (character.Region.IsOppositeParty(otherCharacter.Region) || character.Id == otherCharacter.Id)
							{
								continue;
							}
						}

						crashed = (character.OnSimulateCrash(other) || other.OnSimulateCrash(character)) &&
						          PhysicsUtility.IsOverlapped(new Bounds2D(nextPos, character.Bounds.Size), other.Bounds);

						if (crashed)
						{
							break;
						}
					}

					if (!crashed)
					{
						_bfsQueue.Enqueue((nextNode, firstNode, currentNodeInfo.depth + 1));
					}

					_visitedIds.Add(GetNodeId(nextNode));
				}
			}

			_bfsQueue.Clear();
			_visitedIds.Clear();

			return found;
		}

		private int GetNodeId(Vector2Int node)
		{
			return node.x + 100 + (node.y + 100) * 10000;
		}
	}
}