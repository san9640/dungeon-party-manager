using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Spec;
using UnityEngine;

namespace Dpm.Stage.Unit.AI.Calculator.Move
{
	public class DefaultMoveCalculator : IAIMoveCalculator
	{
		private MoveCalculatorInfo _info;

		private Character _character;

		private Vector2? _targetPos;

		private float _attackableDist;

		public void Init(Character character, MoveCalculatorInfo info)
		{
			_info = info;
			_targetPos = null;
			_character = character;

			_attackableDist = Mathf.Max(_character.BattleAction.Spec.attackRange - AICalculatorConstants.AttackDistEpsilon, 0f);
		}

		public void Dispose()
		{
			_targetPos = null;
			_character = null;
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

			return 0.01f;
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