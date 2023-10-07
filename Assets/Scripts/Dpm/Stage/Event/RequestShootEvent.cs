using Dpm.Stage.Unit;
using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class RequestShootEvent : PooledEvent<RequestShootEvent>
	{
		public IUnit Target { get; private set; }

		public IUnit Shooter { get; private set; }

		public static RequestShootEvent Create(IUnit shooter, IUnit target)
		{
			var e = Pool.GetOrCreate();

			e.Shooter = shooter;
			e.Target = target;

			return e;
		}
	}
}