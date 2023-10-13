using Dpm.Utility.Event;
using UnityEngine;

namespace Dpm.Stage.Event
{
	public class RequestMoveEvent : PooledEvent<RequestMoveEvent>
	{
		public Vector2 TargetPos { get; private set; }

		public static RequestMoveEvent Create(Vector2 targetPos)
		{
			var e = Pool.GetOrCreate();

			e.TargetPos = targetPos;

			return e;
		}
	}
}