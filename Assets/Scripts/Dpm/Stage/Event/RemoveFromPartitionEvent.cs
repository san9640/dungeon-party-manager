using Dpm.Stage.Physics;
using Dpm.Stage.Unit;
using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class RemoveFromPartitionEvent : PooledEvent<RemoveFromPartitionEvent>
	{
		public ICustomCollider Collider { get; private set; }

		public override void Dispose()
		{
			Collider = null;

			base.Dispose();
		}

		public static RemoveFromPartitionEvent Create(ICustomCollider target)
		{
			var e = Pool.GetOrCreate();
			e.Collider = target;

			return e;
		}
	}
}