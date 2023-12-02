using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Spec;
using Dpm.Stage.Unit;
using Dpm.Stage.Unit.AI;
using Dpm.Stage.Unit.AI.Calculator;
using Dpm.Stage.Unit.AI.Calculator.Move;
using Dpm.Utility.Constants;
using Dpm.Utility.Extensions;
using UnityEditor;
using UnityEngine;

public class DistanceFromAlliesMoveCalculator : IAIMoveCalculator
{
    private Character _character;
    private Vector2? _targetPos;
    private const float MinDistance = 4.0f; // 다른 아군 유닛과 최소거리

    public void Init(Character character, MoveCalculatorInfo info)
    {
        _character = character;
    }

    public void Dispose()
    {
        _character = null;
        _targetPos = null;
    }
    
    public float Calculate()
    {
        _targetPos = null;

        var friendlyUnits = StageScene.Instance.UnitManager.AllyParty.Members;
        
        foreach (var unit in friendlyUnits)
        {
            if (unit == _character)
                continue;
            
            var distance = Vector2.Distance(_character.Position, unit.Position);

            if (distance < MinDistance)
            {
                var direction = (_character.Position - unit.Position).normalized;
                _targetPos = _character.Position + direction * MinDistance;
                return 1 / (1 + Mathf.Exp(-MinDistance / distance));
            }
        }

        return AICalculatorConstants.MinInnerScore;
    }

    public Vector2? TargetPos => _targetPos;
    
    public void DrawCurrent()
    {
        AIDebugUtility.DrawMoveAIInfo(_character, _targetPos);
    }
}
