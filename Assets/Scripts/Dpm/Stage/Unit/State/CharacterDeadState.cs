using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Utility.State;

namespace Dpm.Stage.Unit.State
{
	public class CharacterDeadState : PooledState<CharacterDeadState>
	{
		private Character _character;

		public override void Enter()
		{
			_character.Animator.Rotation = 90;

			CoreService.Event.Publish(CharacterEliminatedEvent.Create(_character));
		}

		public override void Exit()
		{
			_character.Animator.Rotation = 0;
		}

		public override void Dispose()
		{
			_character = null;

			base.Dispose();
		}

		public static CharacterDeadState Create(Character character)
		{
			var state = Pool.GetOrCreate();

			state._character = character;

			return state;
		}
	}
}