using Dpm.Stage.Spec;

namespace Dpm.Stage.Unit.AI.Calculator.Attack
{
	public class MeleeTargetAttackCalculator : IAIAttackCalculator
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

			var maxDps = 0f;
			var dpsSum = 0f;

			foreach (var enemy in oppositeParty.Members)
			{
				if (enemy.IsDead)
				{
					continue;
				}

				dpsSum += enemy.BattleAction.Dps;

				if (enemy.BattleAction.Spec.type != BattleActionAttackType.Melee)
				{
					continue;
				}

				if (maxDps < enemy.BattleAction.Dps)
				{
					maxDps = enemy.BattleAction.Dps;
					CurrentTarget = enemy;
				}
			}

			if (CurrentTarget == null)
			{
				return AICalculatorConstants.MinInnerScore;
			}

			return maxDps / dpsSum;
		}

		public void DrawCurrent()
		{
			AIDebugUtility.DrawAttackAIInfo(_character, CurrentTarget);
		}
	}
}