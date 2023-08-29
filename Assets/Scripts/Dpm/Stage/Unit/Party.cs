using System.Collections.Generic;

namespace Dpm.Stage.Unit
{
	public class Party
	{
		public UnitRegion Region { get; private set; }

		public List<Character> Members { get; private set; }

		public Party(UnitRegion region, List<Character> members)
		{
			Region = region;
			Members = members;
		}
	}
}