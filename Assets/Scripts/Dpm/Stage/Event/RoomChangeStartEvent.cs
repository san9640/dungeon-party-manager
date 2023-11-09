using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	/// <summary>
	/// 방 변경 시작 이벤트
	/// </summary>
	public class RoomChangeStartEvent : PooledEvent<RoomChangeStartEvent>
	{
		public int RoomNumber { get; private set; }

		public static RoomChangeStartEvent Create(int roomNumber)
		{
			var e = Pool.GetOrCreate();

			e.RoomNumber = roomNumber;

			return e;
		}
	}
}