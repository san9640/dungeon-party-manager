using Dpm.Stage.Unit;
using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class PartyEliminatedEvent : PooledEvent<PartyEliminatedEvent>
	{
		public Party Party { get; private set; }

		public override void Dispose()
		{
			Party = null;

			base.Dispose();
		}

		public static PartyEliminatedEvent Create(Party party)
		{
			var e = Pool.GetOrCreate();

			e.Party = party;

			return e;
		}
	}
}