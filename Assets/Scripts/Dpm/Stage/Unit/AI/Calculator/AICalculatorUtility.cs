using UnityEngine;

namespace Dpm.Stage.Unit.AI.Calculator
{
	public static class AICalculatorUtility
	{
		public static float ClampScore(float value)
		{
			return Mathf.Clamp(value, AICalculatorConstants.MinInnerScore, AICalculatorConstants.MaxInnerScore);
		}

		/// <summary>
		/// 강한 정도 계산
		/// </summary>
		public static float GetStrengthScore(Character from, Character to)
		{
			// 거리와 상대 속도 체크
			var distScore = 0f;
			var dist = (to.Position - from.Position).magnitude;
			var distAndRangeGap = dist - to.BattleAction.Spec.attackRange;

			if (distAndRangeGap > 0)
			{
				var approachDuration = distAndRangeGap / to.MoveSpeed;
				const float threatenDuration = 1f;
				var threatenScore = threatenDuration / approachDuration;

				if (threatenScore < 1)
				{
					distScore = threatenScore * 0.25f;
				}
			}
			else
			{
				var threatenScore = 1 - dist / to.BattleAction.Spec.attackRange;

				distScore = 0.25f + threatenScore * 0.75f;
			}

			var attackScore = GetDpsGapScore(from.BattleAction.Dps, from.Hp,
				to.BattleAction.Dps, to.Hp);

			return distScore + attackScore;
		}

		public static float GetDpsGapScore(float fromDps, float fromHp, float toDps, float toHp)
		{
			// 상대 공격력과 내 체력 체크
			var killDelay = toHp / fromDps;

			// 상대 체력과 내 공격력 체크
			var deadDelay = fromHp / toDps;

			if (killDelay > deadDelay)
			{
				// 0 ~ 0.5
				var totalDamage = deadDelay * toDps;
				var damageRatio = totalDamage / fromHp;

				// 받을 수 있는 데미지가 높을수록 점수가 높아야 함
				return 0.5f * damageRatio;
			}
			else
			{
				// 0.5 ~ 1.0
				var totalDamage = deadDelay * fromDps;
				var damageRatio = totalDamage / toHp;

				// 넣을 수 있는 데미지가 높을수록 점수가 낮아야 함
				return 0.5f + 0.5f * (1 - damageRatio);
			}
		}
	}
}