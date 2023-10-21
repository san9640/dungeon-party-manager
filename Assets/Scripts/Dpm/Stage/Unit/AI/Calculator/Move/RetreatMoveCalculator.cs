using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Spec;
using Dpm.Utility.Extensions;
using UnityEditor;
using UnityEngine;

namespace Dpm.Stage.Unit.AI.Calculator.Move
{
	public class RetreatMoveCalculator : IAIMoveCalculator
	{
		private MoveCalculatorInfo _info;

		private Character _character;

		private Vector2? _targetPos;

		public Vector2? TargetPos => _targetPos;

		public void Init(Character character, MoveCalculatorInfo info)
		{
			_info = info;
			_targetPos = null;
			_character = character;
		}

		public void Dispose()
		{
			_targetPos = null;
			_character = null;
		}

		public float Calculate()
		{
			_targetPos = null;

			var oppositeParty = UnitManager.Instance.GetOppositeParty(_character.Region);

			var attackPowerSum = Vector2.zero;

			foreach (var enemy in oppositeParty.Members)
			{
				if (enemy.IsDead)
				{
					continue;
				}

				var strengthScore = AICalculatorUtility.GetStrengthScore(_character, enemy);

				var attackDir = strengthScore * (_character.Position - enemy.Position).normalized;

				attackPowerSum += attackDir;
			}

			if (attackPowerSum.IsAlmostZero())
			{
				return AICalculatorConstants.MinInnerScore;
			}

			var myParty = UnitManager.Instance.GetMyParty(_character.Region);

			var totalEnemyHp = 0f;
			var totalEnemyDps = 0f;

			foreach (var enemy in oppositeParty.Members)
			{
				if (enemy.IsDead)
				{
					continue;
				}

				totalEnemyHp += enemy.Hp;
				totalEnemyDps += enemy.BattleAction.Dps;
			}

			var totalAllyHp = 0f;
			var totalAllyDps = 0f;

			foreach (var ally in myParty.Members)
			{
				if (ally.IsDead)
				{
					continue;
				}

				totalAllyHp += ally.Hp;
				totalAllyDps += ally.BattleAction.Dps;
			}

			var dpsGapScore = AICalculatorUtility.GetDpsGapScore(
				totalAllyDps, totalAllyHp, totalEnemyDps, totalEnemyHp);

			var hpScore = 1 - _character.HpRatio;

			var score = dpsGapScore * hpScore;

			_targetPos = _character.Position + attackPowerSum.normalized * 5f;

			return score;
		}

		public void Execute()
		{
			if (!_targetPos.HasValue)
			{
				return;
			}

			CoreService.Event.SendImmediate(_character, RequestMoveEvent.Create(_targetPos.Value));
		}

		public void DrawCurrent()
		{
			AIDebugUtility.DrawMoveAIInfo(_character, _targetPos);
		}
	}
}