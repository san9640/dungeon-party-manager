using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Spec;
using Dpm.Utility.Extensions;
using UnityEditor;
using UnityEngine;

namespace Dpm.Stage.Unit.AI.Calculator.Attack
{
	public class ClosestTargetAttackCalculator : IAIAttackCalculator
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

			var minDist = 9999f;
			var distSum = 0f;
			var aliveTargetCount = 0;

			foreach (var enemy in oppositeParty.Members)
			{
				if (enemy.IsDead)
				{
					continue;
				}

				var toTargetDist = PhysicsUtility.GetDistanceBtwCollider(_character, enemy);

				distSum += toTargetDist;
				aliveTargetCount++;

				if (toTargetDist < minDist)
				{
					minDist = toTargetDist;
					CurrentTarget = enemy;
				}
			}

			if (CurrentTarget == null)
			{
				return AICalculatorConstants.MinInnerScore;
			}

			float score;
			var toTargetGap = minDist - _attackableDist;

			// 공격 가능한 범위에 있으면
			if (toTargetGap < 0)
			{
				score = AICalculatorConstants.MaxInnerScore;
			}
			else
			{
				var averageDist = distSum / aliveTargetCount;
				var attackableToAverage = averageDist - _attackableDist;
				var minToAverage = averageDist - minDist;

				// 무조건 0에서 1 사이의 값이 나올 것이지만, 일단 Clamp해줌
				score = AICalculatorUtility.ClampScore(minToAverage / attackableToAverage);
			}

			return score;
		}

		public void DrawCurrent()
		{
			AIDebugUtility.DrawAttackAIInfo(_character, CurrentTarget);
		}
	}
}