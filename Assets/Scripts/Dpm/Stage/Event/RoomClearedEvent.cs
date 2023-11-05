using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class RoomClearedEvent : PooledEvent<RoomClearedEvent>
	{
		public int DoorIndex { get; private set; }

		public static RoomClearedEvent Create(int doorIndex)
		{
			var e = Pool.GetOrCreate();

			e.DoorIndex = doorIndex;

			return e;
		}
	}
}