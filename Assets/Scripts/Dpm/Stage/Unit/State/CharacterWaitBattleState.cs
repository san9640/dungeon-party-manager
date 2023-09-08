using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Utility.State;

namespace Dpm.Stage.Unit.State
{
	/// <summary>
	/// 전투를 대기하는 State
	/// </summary>
	public class CharacterWaitBattleState : PooledState<CharacterWaitBattleState>
	{
		private Character _character;

		public override void Enter()
		{
			CoreService.Event.Subscribe<BattleStartEvent>(OnBattleStart);
		}

		public override void Exit()
		{
			CoreService.Event.Unsubscribe<BattleStartEvent>(OnBattleStart);
		}

		public override void Dispose()
		{
			_character = null;

			base.Dispose();
		}

		private void OnBattleStart(Core.Interface.Event e)
		{
			// 전투 시작 이벤트를 받으면 전투 State로 전환
			var battleState = CharacterBattleState.Create(_character);

			CoreService.Event.Send(_character, StateChangeEvent.Create(battleState));
		}

		public static CharacterWaitBattleState Create(Character character)
		{
			var state = Pool.GetOrCreate();

			state._character = character;

			return state;
		}
	}
}