using Dpm.Utility.Event;
using Dpm.Utility.State;

namespace Dpm.Stage.Event
{
	public class StateChangeEvent : PooledEvent<StateChangeEvent>
	{
		public IState State { get; private set; }

		public static StateChangeEvent Create(IState state)
		{
			var e = Pool.GetOrCreate();

			e.State = state;

			return e;
		}
	}
}