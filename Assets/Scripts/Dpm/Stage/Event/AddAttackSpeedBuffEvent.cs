using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class AddAttackSpeedBuffEvent : PooledEvent<AddAttackSpeedBuffEvent>
	{
		public float Value { get; private set; }

		public string Key { get; private set; }

		public bool IsPermanent => Key == null;

		public override void Dispose()
		{
			Key = null;

			base.Dispose();
		}

		public static AddAttackSpeedBuffEvent Create(string key, float value)
		{
			var e = Pool.GetOrCreate();

			e.Key = key;
			e.Value = value;

			return e;
		}
	}
}