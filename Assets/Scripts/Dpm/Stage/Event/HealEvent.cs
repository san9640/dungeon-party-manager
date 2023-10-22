using Dpm.Stage.Unit;
using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class HealEvent : PooledEvent<HealEvent>
	{
		public Character Character { get; private set; }

		public int Value { get; private set; }

		public static HealEvent Create(Character character, int value)
		{
			var e = Pool.GetOrCreate();

			e.Character = character;
			e.Value = value;

			return e;
		}
	}
}