using System.Collections;
using System.Collections.Generic;
using Dpm.Stage.Spec;
using Dpm.Stage.UI.Calculator.Attack;
using Dpm.Stage.Unit.AI.Calculator.Attack;
using UnityEngine;

public class ProtectAlliesAttackCalculatorUI : AttackCalculatorUIBase
{
    public override void Init()
    {
        targetSearchingType = AttackTargetSearchingType.ProtectAllies;

        CalculatorType = typeof(ProtectAlliesAttackCalculator);

        base.Init();
    }
}
