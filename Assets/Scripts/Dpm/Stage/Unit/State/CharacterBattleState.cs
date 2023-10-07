using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Utility.State;
using UnityEngine;

namespace Dpm.Stage.Unit.State
{
	/// <summary>
	/// 전투중인 캐릭터의 State
	/// </summary>
	public class CharacterBattleState : PooledState<CharacterBattleState>, IUpdatable
	{
		private Character _character;

		private float _testTimePassed;

		public override void Enter()
		{
			CoreService.FrameUpdate.RegisterUpdate(this, _character.Id);

			CoreService.Event.Subscribe<BattleEndEvent>(OnBattleEnd);

			// 캐릭터는 전투 중일 때에만 파티션에 넣는다
			CoreService.Event.PublishImmediate(AddToPartitionEvent.Create(_character));
		}

		public override void Exit()
		{
			CoreService.Event.PublishImmediate(RemoveFromPartitionEvent.Create(_character));

			CoreService.Event.Unsubscribe<BattleEndEvent>(OnBattleEnd);

			CoreService.FrameUpdate.UnregisterUpdate(this, _character.Id);
		}

		public override void Dispose()
		{
			_character = null;

			base.Dispose();
		}

		public void UpdateFrame(float dt)
		{
			_character.DecisionMaker.UpdateFrame(dt);

			// FIXME : 테스트 코드
			if (_character.Region == UnitRegion.Ally)
			{
				var dist = dt * 3;
				var xMove = Input.GetAxis("Horizontal");
				var yMove = Input.GetAxis("Vertical");

				var moveDir = new Vector2(xMove, yMove).normalized;

				StagePhysicsManager.Instance.Move(_character, moveDir, dist);
			}
			else if (_character.Region == UnitRegion.Enemy)
			{
				_testTimePassed += dt;

				if (_testTimePassed > 1)
				{
					var enemyParty = UnitManager.Instance.AllyParty.Members;

					var target = enemyParty[Random.Range(0, enemyParty.Count)];

					ProjectileManager.Instance.Shoot("bolt", _character, target);

					_testTimePassed -= 1;
				}

				for (var key = KeyCode.Alpha1; key <= KeyCode.Alpha4; key++)
				{
					if (Input.GetKeyDown(key) &&
					    ReferenceEquals(_character, UnitManager.Instance.EnemyParty.Members[key - KeyCode.Alpha1]))
					{
						var deadState = CharacterDeadState.Create(_character);

						CoreService.Event.Send(_character, StateChangeEvent.Create(deadState));

						break;
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