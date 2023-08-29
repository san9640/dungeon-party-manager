using Dpm.Utility.Event;

namespace Dpm.Common.Event
{
	public class ScreenFadeInEndEvent : PooledEvent<ScreenFadeInEndEvent>
	{
		public object Requester { get; private set; }

		public override void Dispose()
		{
			Requester = null;

			base.Dispose();
		}

		public static ScreenFadeInEndEvent Create(object requester)
		{
			var e = Pool.GetOrCreate();
			e.Requester = requester;

			return e;
		}
	}
}