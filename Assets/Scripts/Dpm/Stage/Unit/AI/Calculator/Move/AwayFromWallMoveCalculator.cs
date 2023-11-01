using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Spec;
using Dpm.Utility.Constants;
using UnityEngine;

namespace Dpm.Stage.Unit.AI.Calculator.Move
{
	public class AwayFromWallMoveCalculator : IAIMoveCalculator
	{
		private MoveCalculatorInfo _info;

		private Character _character;

		private Vector2? _targetPos;

		public Vector2? TargetPos => _targetPos;

		private Bounds2D _maxScorePosZone;

		private Bounds2D _minScorePosZone;

		private const float TargetMoveDist = 3.0f;

		public void Init(Character character, MoveCalculatorInfo info)
		{
			_character = character;
			_info = info;

			var cornerOffset = character.Bounds.extents + GameConstants.Epsilon * Vector2.one;
			var battleZone = StageScene.Instance.BattleZone;

			_maxScorePosZone = new Bounds2D(battleZone.center, battleZone.Size - cornerOffset * 2);
			_minScorePosZone = new Bounds2D(battleZone.center, _maxScorePosZone.Size * 0.85f);
		}

		public void Dispose()
		{
			_character = null;
			_targetPos = null;
		}

		public float Calculate()
		{
			_targetPos = null;

			float? minXFromBoundary = null;

			if (_character.Position.x < _minScorePosZone.Min.x)
			{
				minXFromBoundary = _character.Position.x - _maxScorePosZone.Min.x;
			}
			else if (_character.Position.x > _minScorePosZone.Max.x)
			{
				minXFromBoundary = _maxScorePosZone.Min.x - _character.Position.x;
			}

			float? minYFromBoundary = null;

			if (_character.Position.y < _minScorePosZone.Min.y)
			{
				minYFromBoundary = _character.Position.y - _maxScorePosZone.Min.y;
			}
			else if (_character.Position.y > _minScorePosZone.Max.y)
			{
				minYFromBoundary = _maxScorePosZone.Min.y - _character.Position.y;
			}

			if (minXFromBoundary == null && minYFromBoundary == null)
			{
				return 0;
			}

			var minMaxGap = _maxScorePosZone.extents - _minScorePosZone.extents;
			var minXValue = minXFromBoundary ?? minMaxGap.x;
			var minYValue = minYFromBoundary ?? minMaxGap.y;

			// 선형으로 계산하도록 했으나, 문제가 생기는 경우 다른 계산식으로 변경 필요
			var xMoveScore = 1 - minXValue / minMaxGap.x;
			var yMoveScore = 1 - minYValue / minMaxGap.y;

			var score = xMoveScore + yMoveScore;
			Vector2 dir;

			if (xMoveScore < yMoveScore)
			{
				dir = _character.Position.y > StageScene.Instance.BattleZone.center.y ? Vector2.down : Vector2.up;
			}
			else
			{
				dir = _character.Position.x > StageScene.Instance.BattleZone.center.x ? Vector2.left : Vector2.right;
			}

			_targetPos = _character.Position + dir * TargetMoveDist;

			return score;
		}

		public void DrawCurrent()
		{
			AIDebugUtility.DrawMoveAIInfo(_character, _targetPos);
		}
	}
}