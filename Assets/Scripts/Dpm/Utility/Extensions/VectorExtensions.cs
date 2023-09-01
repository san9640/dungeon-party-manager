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
	}
}