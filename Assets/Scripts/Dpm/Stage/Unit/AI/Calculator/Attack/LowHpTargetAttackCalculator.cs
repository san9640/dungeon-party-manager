using Dpm.Stage.Spec;

namespace Dpm.Stage.Unit.AI.Calculator.Attack
{
	public class LowHpTargetAttackCalculator : IAIAttackCalculator
	{
		private AttackCalculatorInfo _info;

		private Character _character;

		public IUnit CurrentTarget { get; private set; }

		public void Init(Character character, AttackCalculatorInfo info)
		{
			_character = character;
			_info = info;
			CurrentTarget = null;
		}

		public void Dispose()
		{
			_character = null;
			CurrentTarget = null;
		}

		public float Calculate()
		{
			CurrentTarget = null;

			var oppositeParty = UnitManager.Instance.GetOppositeParty(_character.Region);

			if (oppositeParty == null)
			{
				return AICalculatorConstants.MinInnerScore;
			}

			var lowestRatio = 1f;

			foreach (var enemy in oppositeParty.Members)
			{
				if (enemy.IsDead)
				{
					continue;
				}

				if (enemy.HpRatio < lowestRatio)
				{
					lowestRatio = enemy.HpRatio;
					CurrentTarget = enemy;
				}
			}

			if (CurrentTarget == null)
			{
				return AICalculatorConstants.MinInnerScore;
			}

			return (1 - lowestRatio) * (1 - lowestRatio);
		}

		public void DrawCurrent()
		{
			AIDebugUtility.DrawAttackAIInfo(_character, CurrentTarget);
		}
	}
}