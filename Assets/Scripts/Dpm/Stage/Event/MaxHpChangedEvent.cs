﻿using Dpm.Stage.Unit;
using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class MaxHpChangedEvent : PooledEvent<MaxHpChangedEvent>
	{
		public Character Character { get; private set; }

		public override void Dispose()
		{
			Character = null;

			base.Dispose();
		}

		public static MaxHpChangedEvent Create(Character character)
		{
			var e = Pool.GetOrCreate();

			e.Character = character;

			return e;
		}
	}
}