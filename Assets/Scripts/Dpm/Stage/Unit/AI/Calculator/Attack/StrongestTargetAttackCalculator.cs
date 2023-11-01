using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Spec;
using UnityEngine;

namespace Dpm.Stage.Unit.AI.Calculator.Attack
{
	public class StrongestTargetAttackCalculator : IAIAttackCalculator
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

			var maxStrength = -1f;
			Character maxStrengthEnemy = null;

			foreach (var enemy in oppositeParty.Members)
			{
				if (enemy.IsDead)
				{
					continue;
				}

				var strength = AICalculatorUtility.GetStrengthScore(_character, enemy);

				if (strength > maxStrength)
				{
					maxStrength = strength;

					maxStrengthEnemy = enemy;
				}
			}

			if (maxStrength < 0 || maxStrengthEnemy == null)
			{
				return AICalculatorConstants.MinInnerScore;
			}

			CurrentTarget = maxStrengthEnemy;

			return maxStrengthEnemy.HpRatio;

			// var myParty = UnitManager.Instance.GetMyParty(_character.Region);
			//
			// var totalEnemyHp = 0f;
			// var totalEnemyDps = 0f;
			//
			// foreach (var enemy in oppositeParty.Members)
			// {
			// 	if (enemy.IsDead)
			// 	{
			// 		continue;
			// 	}
			//
			// 	totalEnemyHp += enemy.Hp;
			// 	totalEnemyDps += enemy.BattleAction.Dps;
			// }
			//
			// var totalAllyHp = 0f;
			// var totalAllyDps = 0f;
			//
			// foreach (var ally in myParty.Members)
			// {
			// 	if (ally.IsDead)
			// 	{
			// 		continue;
			// 	}
			//
			// 	totalAllyHp += ally.Hp;
			// 	totalAllyDps += ally.BattleAction.Dps;
			// }
			//
			// var dpsGapScore = AICalculatorUtility.GetDpsGapScore(
			// 	totalAllyDps, totalAllyHp, totalEnemyDps, totalEnemyHp);
			//
			// var hpScore = _character.HpRatio;
			//
			// var score = (1 - dpsGapScore) * hpScore;
			//
			// return score;


		}

		public void DrawCurrent()
		{
			AIDebugUtility.DrawAttackAIInfo(_character, CurrentTarget);
		}
	}
}