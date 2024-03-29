﻿using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Spec;
using Dpm.Utility.Constants;
using Dpm.Utility.Extensions;
using UnityEditor;
using UnityEngine;

namespace Dpm.Stage.Unit.AI.Calculator.Move
{
	public class ApproachToEnemyMoveCalculator : IAIMoveCalculator
	{
		private MoveCalculatorInfo _info;

		private Character _character;

		private Vector2? _targetPos;

		public Vector2? TargetPos => _targetPos;

		private const float MaxScoreMinDuration = 0.5f;

		private float _attackableDist;

		public void Init(Character character, MoveCalculatorInfo info)
		{
			_info = info;
			_character = character;

			_attackableDist = Mathf.Max(_character.BattleAction.Spec.attackRange - AICalculatorConstants.AttackDistEpsilon, 0f);
		}

		public void Dispose()
		{
			_character = null;
			_targetPos = null;
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

			_targetPos = currentAttackTarget.Position;

			var requiredMoveDuration = requiredMoveDist / _character.MoveSpeed;
			var score = AICalculatorUtility.ClampScore(MaxScoreMinDuration / requiredMoveDuration);

			return score;
		}

		public void DrawCurrent()
		{
			AIDebugUtility.DrawMoveAIInfo(_character, _targetPos);
		}
	}
}