using Dpm.Stage.Unit;
using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class HpChangedEvent : PooledEvent<HpChangedEvent>
	{
		public Character Character { get; private set; }

		public override void Dispose()
		{
			Character = null;

			base.Dispose();
		}

		public static HpChangedEvent Create(Character character)
		{
			var e = Pool.GetOrCreate();

			e.Character = character;

			return e;
		}
	}
}