using System;
using Dpm.Stage.Unit;
using Dpm.Utility.Event;

namespace Dpm.Stage.Event
{
	public class ChangeAICalculatorFactorEvent : PooledEvent<ChangeAICalculatorFactorEvent>
	{
		public Character Character { get; private set; }

		public Type AICalculatorType { get; private set; }

		public float Value { get; private set; }

		public override void Dispose()
		{
			Character = null;
			AICalculatorType = null;

			base.Dispose();
		}

		public static ChangeAICalculatorFactorEvent Create(Character character, Type aiCalculatorType, float value)
		{
			var e = Pool.GetOrCreate();

			e.Character = character;
			e.AICalculatorType = aiCalculatorType;
			e.Value = value;

			return e;
		}
	}
}