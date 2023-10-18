using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Utility.State;
using UnityEngine;

namespace Dpm.Stage.Unit.State
{
	/// <summary>
	/// 전투에 이긴 파티의 상태. 아이템 등을 캐릭터에게 붙일 수 있음
	/// 해당 스테이트는 이긴 애들만 가지고 있어야 함
	/// </summary>
	public class CharacterAfterBattleState : PooledState<CharacterAfterBattleState>, IUpdatable
	{
		private Character _character;

		public override void Enter()
		{
			// TODO : 대충 방방 뛰고있자

			CoreService.Event.Subscribe<RoomChangeStartEvent>(OnRoomChangeStart);
		}

		public override void Exit()
		{
			CoreService.Event.Unsubscribe<RoomChangeStartEvent>(OnRoomChangeStart);
		}

		public override void Dispose()
		{
			_character = null;

			base.Dispose();
		}

		private void OnRoomChangeStart(Core.Interface.Event e)
		{
			var waitBattleState = CharacterWaitBattleState.Create(_character);

			CoreService.Event.Send(_character, StateChangeEvent.Create(waitBattleState));
		}

		public static CharacterAfterBattleState Create(Character character)
		{
			var state = Pool.GetOrCreate();

			state._character = character;

			return state;
		}

		public void UpdateFrame(float dt)
		{
			// FIXME : 테스트용 코드
			if (_character.Region == UnitRegion.Ally)
			{
				var dist = dt * 3;
				var xMove = Input.GetAxis("Horizontal");
				var yMove = Input.GetAxis("Vertical");

				var moveDir = new Vector2(xMove, yMove).normalized;

				StagePhysicsManager.Instance.Move(_character, moveDir, dist);

				if (Input.GetKeyDown(KeyCode.Q))
				{
					var info = new ProjectileInfo
					{
						damage = 0,
						shooter = _character,
						targetPos = _character.Position + Vector2.left
					};

					ProjectileManager.Instance.Shoot("bolt", info);
				}
			}
		}
	}
}