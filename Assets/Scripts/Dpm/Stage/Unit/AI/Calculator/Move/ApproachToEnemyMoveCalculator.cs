using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Spec;
using Dpm.Utility.Constants;
using UnityEngine;

namespace Dpm.Stage.Unit.AI.Calculator.Move
{
	public class ApproachToEnemyMoveCalculator : IAIMoveCalculator
	{
		private MoveCalculatorInfo _info;

		private Character _character;

		private Vector2? _targetPos;

		private const float MaxScoreMinDuration = 0.5f;

		private float _attackableDist;

		public void Init(Character character, MoveCalculatorInfo info)
		{
			_info = info;
			_character = character;

			_attackableDist = Mathf.Max(_character.BattleAction.Spec.attackRange - AICalculatorConstants.AttackDistEpsilon, 0f);
		}

		public float Calculate()
		{
			_targetPos = null;

			var currentAttackTarget = _character.DecisionMaker.CurrentAttackTarget;

			if (currentAttackTarget == null)
			{
				return AICalculatorConstants.MinInnerScore;
			}

			var toTargetDist = PhysicsUtility.GetDistanceBtwCollider(_character, currentAttackTarget);
			var requiredMoveDist = toTargetDist - _attackableDist;

			// 공격할 수 있게 함
			if (requiredMoveDist < 0)
			{
				return AICalculatorConstants.MinInnerScore;
			}

			var toTargetDir = (currentAttackTarget.Position - _character.Position).normalized;

			_targetPos = requiredMoveDist * toTargetDir + _character.Position;

			var requiredMoveDuration = requiredMoveDist / _character.MoveSpeed;
			var score = AICalculatorUtility.ClampScore(MaxScoreMinDuration / requiredMoveDuration);

			return score * _info.weightFactorInfo.defaultValue;
		}

		public void Execute()
		{
			if (!_targetPos.HasValue)
			{
				return;
			}

			CoreService.Event.SendImmediate(_character, RequestMoveEvent.Create(_targetPos.Value));
		}

		public void Dispose()
		{
			_character = null;
			_targetPos = null;
		}
	}
}