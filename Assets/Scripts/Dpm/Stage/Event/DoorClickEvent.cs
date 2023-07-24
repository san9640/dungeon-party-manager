using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class DoorClickEvent : PooledEvent<DoorClickEvent>
	{
		public int DoorIndex { get; private set; }

		public static DoorClickEvent Create(int doorIndex)
		{
			var e = Pool.GetOrCreate();

			e.DoorIndex = doorIndex;

			return e;
		}
	}
}