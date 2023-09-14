﻿using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Utility.State;

namespace Dpm.Stage.Unit.State
{
	/// <summary>
	/// 전투에 이긴 파티의 상태. 아이템 등을 캐릭터에게 붙일 수 있음
	/// 해당 스테이트는 이긴 애들만 가지고 있어야 함
	/// </summary>
	public class CharacterAfterBattleState : PooledState<CharacterAfterBattleState>
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
	}
}