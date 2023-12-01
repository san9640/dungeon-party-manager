using System.Collections;
using System.Collections.Generic;
using Dpm.Stage.Spec;
using Dpm.Stage.UI.Calculator.Move;
using UnityEngine;

public class DistanceFromAlliesMoveCalculatorUI : MoveCalculatorUIBase
{
    public override void Init()
    {
        moveType = MoveType.DistanceFromAllies;

        CalculatorType = typeof(DistanceFromAlliesMoveCalculator);

        base.Init();
    }
}
