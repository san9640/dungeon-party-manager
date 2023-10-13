using Dpm.Stage.Unit;
using Dpm.Utility.Event;
using UnityEngine;

namespace Dpm.Stage.Event
{
	public class DamageEvent : PooledEvent<DamageEvent>
	{
		public int Damage { get; private set; }

		public IUnit Sender { get; private set; }

		public static DamageEvent Create(IUnit sender, int damage)
		{
			var e = Pool.GetOrCreate();

			e.Sender = sender;
			e.Damage = damage;

			return e;
		}

		public static DamageEvent Create(IUnit sender, float damage)
		{
			var e = Pool.GetOrCreate();

			e.Sender = sender;
			e.Damage = Mathf.CeilToInt(damage);

			return e;
		}
	}
}