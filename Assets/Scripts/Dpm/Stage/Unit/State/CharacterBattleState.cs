using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Utility.Extensions;
using Dpm.Utility.State;
using UnityEngine;

namespace Dpm.Stage.Unit.State
{
	/// <summary>
	/// 전투중인 캐릭터의 State
	/// </summary>
	public class CharacterBattleState : PooledState<CharacterBattleState>, IUpdatable, IEventListener
	{
		private Character _character;

		private float _testTimePassed;

		private Vector2? _moveTargetPos;

		private bool _needPathFinding;

		public override void Enter()
		{
			CoreService.Event.Subscribe<BattleEndEvent>(OnBattleEnd);

			// 캐릭터는 전투 중일 때에만 파티션에 넣는다
			CoreService.Event.PublishImmediate(AddToPartitionEvent.Create(_character));

			_character.BattleAction?.Reset();
		}

		public override void Exit()
		{
			CoreService.Event.PublishImmediate(RemoveFromPartitionEvent.Create(_character));

			CoreService.Event.Unsubscribe<BattleEndEvent>(OnBattleEnd);

			_character.Animator.RemoveAnimation();
		}

		public override void Dispose()
		{
			_character = null;
			_moveTargetPos = null;

			base.Dispose();
		}

		public void OnEvent(Core.Interface.Event e)
		{
			_character.BattleAction.OnEvent(e);

			if (e is RequestMoveEvent rme)
			{
				_moveTargetPos = rme.TargetPos;
				_needPathFinding = rme.FindingPath;
			}
		}

		public void UpdateFrame(float dt)
		{
			_character.DecisionMaker.UpdateFrame(dt);
			(_character.BattleAction as IUpdatable)?.UpdateFrame(dt);

			if (_moveTargetPos.HasValue)
			{
				var diff = _moveTargetPos.Value - _character.Position;

				if (diff.IsAlmostZero())
				{
					_moveTargetPos = null;
					_character.Animator.RemoveAnimation();
				}
				else
				{
					_character.Animator.SetAnimation("run");

					var moveDir = diff.normalized;
					var leftoverDist = diff.magnitude;
					var prevPos = _character.Position;

					var moveDist = Mathf.Min(leftoverDist, dt * _character.MoveSpeed);

					var result = StagePhysicsManager.Instance.Move(_character, moveDir, moveDist);

					// 다른 유닛이랑 충돌했을 경우에만 방향을 조금 틀어줌
					if (_needPathFinding && result.crasher is IUnit)
					{
						var blockedDist = moveDist - (_character.Position - prevPos).magnitude;

						// 물리 계산이 완벽하지 않아서, blockedDist가 0보다 작을 수도 있음...
						if (blockedDist > 0 &&
						    StagePhysicsManager.Instance.TryFindPath(_character, _moveTargetPos.Value, out var pathDir))
						{
							StagePhysicsManager.Instance.Move(_character, pathDir, blockedDist);
						}
					}
				}
			}
		}

		private void OnBattleEnd(Core.Interface.Event e)
		{
			if (e is not BattleEndEvent bee)
			{
				return;
			}

			// 이런 일은 존재하지 않아야 함...!
			if (bee.WonPartyRegion != _character.Region)
			{
				Debug.LogError($"Battle was done but [Character : {_character.name}] alive in [Region : { _character.Region }]");
				return;
			}

			var afterBattleState = CharacterAfterBattleState.Create(_character);

			CoreService.Event.Send(_character, StateChangeEvent.Create(afterBattleState));
		}

		public static CharacterBattleState Create(Character character)
		{
			var state = Pool.GetOrCreate();

			state._character = character;

			return state;
		}
	}
}