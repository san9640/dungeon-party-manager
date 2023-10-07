using System;

namespace Dpm.Stage.Unit
{
	[Flags]
	public enum UnitRegion
	{
		None = 0,
		Ally = 1 << 0,
		Enemy = 1 << 1,
		Neutral = 1 << 2,
		Creature = Ally | Enemy,
		All = Ally | Enemy | Neutral,
	}

	public static class UnitRegionExtensions
	{
		public static bool IsOppositeParty(this UnitRegion me, UnitRegion other)
		{
			if ((me & other) != UnitRegion.None)
			{
				return false;
			}

			return (me | other) == UnitRegion.Creature;
		}
	}
}