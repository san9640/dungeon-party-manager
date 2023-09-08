using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Utility.State;

namespace Dpm.Stage.Unit.State
{
	public class UnitIdleState : PooledState<UnitIdleState>
	{
		private IUnit _unit;

		public override void Enter()
		{
			CoreService.Event.PublishImmediate(AddToPartitionEvent.Create(_unit));
		}

		public override void Exit()
		{
			CoreService.Event.PublishImmediate(RemoveFromPartitionEvent.Create(_unit));
		}

		public override void Dispose()
		{
			_unit = null;

			base.Dispose();
		}

		public static UnitIdleState Create(IUnit unit)
		{
			var state = Pool.GetOrCreate();

			state._unit = unit;

			return state;
		}
	}
}