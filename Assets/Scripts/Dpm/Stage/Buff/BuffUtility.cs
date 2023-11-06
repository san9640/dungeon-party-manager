using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Spec;
using Dpm.Stage.Unit;

namespace Dpm.Stage.Buff
{
	public static class BuffUtility
	{
		public static void ApplyBuff(Character character, string buffSpecName)
		{
			var buff = SpecUtility.GetSpec<BuffSpec>(buffSpecName);

			ApplyBuff(character, buff);
		}

		public static void ApplyBuff(Character character, BuffSpec buff)
		{
			Core.Interface.Event buffEvent = buff.type switch
			{
				BuffType.AttackSpeed => AddAttackSpeedBuffEvent.Create(null, buff.value),
				BuffType.Damage => AddDamageBuffEvent.Create(null, buff.value),
				BuffType.MaxHp => AddMaxHpBuffEvent.Create(null, buff.IntValue),
				_ => null
			};

			if (buffEvent != null)
			{
				CoreService.Event.SendImmediate(character, buffEvent);
			}
		}
	}
}