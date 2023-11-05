using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class AddMaxHpBuffEvent : PooledEvent<AddMaxHpBuffEvent>
	{
		public string Key { get; private set; }

		public int Value { get; private set; }

		public bool IsPermanent => Key == null;

		public override void Dispose()
		{
			Key = null;

			base.Dispose();
		}

		public static AddMaxHpBuffEvent Create(string key, int value)
		{
			var e = Pool.GetOrCreate();

			e.Key = key;
			e.Value = value;

			return e;
		}
	}
}