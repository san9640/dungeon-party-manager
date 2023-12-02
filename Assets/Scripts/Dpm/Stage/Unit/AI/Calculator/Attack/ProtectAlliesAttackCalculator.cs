using System.Collections;
using System.Collections.Generic;
using Dpm.Stage.Spec;
using Dpm.Stage.Unit;
using Dpm.Stage.Unit.AI;
using Dpm.Stage.Unit.AI.Calculator.Attack;
using UnityEngine;

public class ProtectAlliesAttackCalculator : IAIAttackCalculator
{
    private Character _character;
    private IUnit _currentTarget;

    public IUnit CurrentTarget => _currentTarget;

    public void Init(Character character, AttackCalculatorInfo info)
    {
        _character = character;
    }

    public void Dispose()
    {
        _character = null;
        _currentTarget = null;
    }

    public float Calculate()
    {
        _currentTarget = null;

        var friendlyUnits = UnitManager.Instance.AllyParty.Members;
        float maxScore = 0;
        float score = 0;

        foreach (var unit in friendlyUnits)
        {
            if (unit == _character)
                continue;
            
            var enemies = UnitManager.Instance.GetOppositeParty(unit.Region).Members;
            foreach (var enemy in enemies)
            {
                if (enemy.IsDead)
                {
                    continue;
                }
                
                score = 0;
                
                if ((Character)enemy.CurrentAttackTarget == unit)
                {
                    score += (1 - unit.HpRatio);

                    if (score > maxScore)
                    {
                        _currentTarget = enemy;
                        maxScore = score;
                    }
                }
            }
        }

        return maxScore;
    }

    public void DrawCurrent()
    {
        AIDebugUtility.DrawAttackAIInfo(_character, _currentTarget);
    }
}
