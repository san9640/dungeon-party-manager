using Dpm.Stage.Unit;
using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class RequestAttackTargetEvent : PooledEvent<RequestAttackTargetEvent>
	{
		public IUnit Target { get; private set; }

		public static RequestAttackTargetEvent Create(IUnit target)
		{
			var e = Pool.GetOrCreate();

			e.Target = target;

			return e;
		}
	}
}