using UnityEngine;

namespace Dpm.Stage.Physics
{
	public struct MoveResult
	{
		// FIXME : Crasher는 여러 개가 존재할 수 있음
		public ICustomCollider crasher;

		public Vector2 endPos;
	}
}