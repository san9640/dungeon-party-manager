using Dpm.Utility.Constants;

namespace Dpm.Utility.Extensions
{
	public static class FloatExtensions
	{
		public static bool IsAlmostZero(this float value)
		{
			return value < GameConstants.Epsilon;
		}
	}
}