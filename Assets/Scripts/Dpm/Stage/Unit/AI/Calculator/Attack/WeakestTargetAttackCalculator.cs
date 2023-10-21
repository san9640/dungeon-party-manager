using Dpm.Stage.Spec;
using UnityEngine;

namespace Dpm.Stage.Unit.AI.Calculator.Attack
{
	public class WeakestTargetAttackCalculator : IAIAttackCalculator
	{
		private AttackCalculatorInfo _info;

		private Character _character;

		public IUnit CurrentTarget { get; private set; }

		private float _attackableDist;

		public void Init(Character character, AttackCalculatorInfo info)
		{
			_character = character;
			_info = info;
			CurrentTarget = null;

			_attackableDist = Mathf.Max(
				_character.BattleAction.Spec.attackRange - AICalculatorConstants.AttackDistEpsilon, 0f);
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

			var minStrength = 2f;
			Character minStrengthEnemy = null;

			foreach (var enemy in oppositeParty.Members)
			{
				if (enemy.IsDead)
				{
					continue;
				}

				var strength = AICalculatorUtility.GetStrengthScore(_character, enemy);

				if (strength < minStrength)
				{
					minStrength = strength;

					minStrengthEnemy = enemy;
				}
			}

			if (minStrength > 1.5f || minStrengthEnemy == null)
			{
				return AICalculatorConstants.MinInnerScore;
			}

			CurrentTarget = minStrengthEnemy;

			return 1 - minStrengthEnemy.HpRatio;
		}

		public void Execute()
		{

		}
		public void DrawCurrent()
		{
		}
	}
}