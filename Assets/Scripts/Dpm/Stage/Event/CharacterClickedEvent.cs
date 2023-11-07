using Dpm.Stage.Unit;
using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class CharacterClickedEvent : PooledEvent<CharacterClickedEvent>
	{
		public Character Character { get; private set; }

		public bool IsDown { get; private set; }

		public override void Dispose()
		{
			Character = null;

			base.Dispose();
		}

		public static CharacterClickedEvent Create(Character character, bool isDown)
		{
			var e = Pool.GetOrCreate();

			e.Character = character;
			e.IsDown = isDown;

			return e;
		}
	}
}