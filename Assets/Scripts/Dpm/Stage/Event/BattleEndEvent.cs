using Dpm.Stage.Unit;
using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class BattleEndEvent : PooledEvent<BattleEndEvent>
	{
		public UnitRegion WonPartyRegion { get; private set; }

		public override void Dispose()
		{
			WonPartyRegion = UnitRegion.Neutral;

			base.Dispose();
		}

		public static BattleEndEvent Create(UnitRegion wonPartyRegion)
		{
			var e = Pool.GetOrCreate();
			e.WonPartyRegion = wonPartyRegion;

			return e;
		}
	}
}