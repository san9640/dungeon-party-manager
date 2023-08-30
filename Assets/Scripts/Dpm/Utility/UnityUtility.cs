using UnityEngine;

namespace Dpm.Utility
{
	public static class UnityUtility
	{
		public static bool IsDestroyedOrNull(this Object obj)
		{
			return ReferenceEquals(obj, null);
		}
	}
}