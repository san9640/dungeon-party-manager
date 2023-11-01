using Dpm.Stage.Spec;

namespace Dpm.Stage.Unit.AI.Calculator.Attack
{
	public class HighHpTargetAttackCalculator : IAIAttackCalculator
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

			var highestRatio = -1.0f;

			foreach (var enemy in oppositeParty.Members)
			{
				if (enemy.IsDead)
				{
					continue;
				}

				if (highestRatio < enemy.HpRatio)
				{
					highestRatio = enemy.HpRatio;
					CurrentTarget = enemy;
				}
			}

			if (CurrentTarget == null)
			{
				return AICalculatorConstants.MinInnerScore;
			}

			return highestRatio * highestRatio;
		}

		public void DrawCurrent()
		{
			AIDebugUtility.DrawAttackAIInfo(_character, CurrentTarget);
		}
	}
}