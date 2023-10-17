using Dpm.Utility.Event;
using UnityEngine;

namespace Dpm.Stage.Event
{
	public class RequestMoveEvent : PooledEvent<RequestMoveEvent>
	{
		public Vector2 TargetPos { get; private set; }

		public bool FindingPath { get; private set; }

		public static RequestMoveEvent Create(Vector2 targetPos, bool findingPath = true)
		{
			var e = Pool.GetOrCreate();

			e.TargetPos = targetPos;
			e.FindingPath = findingPath;

			return e;
		}
	}
}