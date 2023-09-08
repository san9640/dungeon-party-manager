using Dpm.Utility.Constants;
using UnityEngine;

namespace Dpm.Utility.Extensions
{
	public static class Vector2Extensions
	{
		public static bool IsAlmostZero(this Vector2 value)
		{
			return value.sqrMagnitude < GameConstants.Epsilon * GameConstants.Epsilon;
		}

		public static Vector3 ConvertToVector3(this Vector2 value)
		{
			return new Vector3(value.x, value.y, 0);
		}
	}

	public static class Vector3Extensions
	{
		public static Vector2 ConvertToVector2(this Vector3 value)
		{
			return new Vector2(value.x, value.y);
		}
	}
}