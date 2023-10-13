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
				}
				else
				{
					var moveDir = diff.normalized;
					var leftoverDist = diff.magnitude;
					var prevPos = _character.Position;

					var moveDist = Mathf.Min(leftoverDist, dt * _character.MoveSpeed);

					var result = StagePhysicsManager.Instance.Move(_character, moveDir, moveDist);

					// 다른 유닛이랑 충돌했을 경우에만 방향을 조금 틀어줌
					if (result.crasher is IUnit)
					{
						var targetPos = moveDist * moveDir;
						var blockedDist = moveDist - (targetPos - prevPos).magnitude;

						// 물리 계산이 완벽하지 않아서, blockedDist가 0보다 작을 수도 있음...
						if (blockedDist > 0)
						{
							var toCrasher = result.crasher.Position - _character.Position;

							if (Mathf.Abs(toCrasher.x) > Mathf.Abs(toCrasher.y))
							{
								moveDir = Vector2.right * (moveDir.y > 0 ? 1 : -1);
							}
							else
							{
								moveDir = Vector2.up * (moveDir.x > 0 ? 1 : -1);
							}

							StagePhysicsManager.Instance.Move(_character, moveDir, blockedDist);
						}
					}
				}
			}

			// // FIXME : 테스트 코드
			// if (_character.Region == UnitRegion.Ally)
			// {
			// 	var dist = dt * 3;
			// 	var xMove = Input.GetAxis("Horizontal");
			// 	var yMove = Input.GetAxis("Vertical");
			//
			// 	var moveDir = new Vector2(xMove, yMove).normalized;
			//
			// 	StagePhysicsManager.Instance.Move(_character, moveDir, dist);
			// }
			// else if (_character.Region == UnitRegion.Enemy)
			// {
			// 	_testTimePassed += dt;
			//
			// 	if (_testTimePassed > 1)
			// 	{
			// 		var enemyParty = UnitManager.Instance.AllyParty.Members;
			//
			// 		var target = enemyParty[Random.Range(0, enemyParty.Count)];
			//
			// 		ProjectileManager.Instance.Shoot("bolt", new ProjectileInfo
			// 		{
			// 			target =
			// 		});
			//
			// 		_testTimePassed -= 1;
			// 	}
			//
			// 	for (var key = KeyCode.Alpha1; key <= KeyCode.Alpha4; key++)
			// 	{
			// 		if (Input.GetKeyDown(key) &&
			// 		    ReferenceEquals(_character, UnitManager.Instance.EnemyParty.Members[key - KeyCode.Alpha1]))
			// 		{
			// 			var deadState = CharacterDeadState.Create(_character);
			//
			// 			CoreService.Event.Send(_character, StateChangeEvent.Create(deadState));
			//
			// 			break;
			// 		}
			// 	}
			// }
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