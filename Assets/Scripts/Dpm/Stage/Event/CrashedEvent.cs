using Dpm.Stage.Physics;
using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class CrashedEvent : PooledEvent<CrashedEvent>
	{
		public ICustomCollider Crasher { get; private set; }

		public static CrashedEvent Create(ICustomCollider crasher)
		{
			var e = Pool.GetOrCreate();

			e.Crasher = crasher;

			return e;
		}
	}
}