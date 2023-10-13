using Dpm.Stage.Unit;
using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class RequestMoveProjectileEvent : PooledEvent<RequestMoveProjectileEvent>
	{
		public ProjectileInfo Info { get; private set; }

		public static RequestMoveProjectileEvent Create(ProjectileInfo info)
		{
			var e = Pool.GetOrCreate();

			e.Info = info;

			return e;
		}
	}
}