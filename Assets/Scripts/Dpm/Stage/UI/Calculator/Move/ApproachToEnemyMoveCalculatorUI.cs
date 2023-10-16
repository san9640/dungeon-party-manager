using Dpm.Stage.Spec;
using Dpm.Stage.Unit.AI.Calculator.Move;

namespace Dpm.Stage.UI.Calculator.Move
{
	public class ApproachToEnemyMoveCalculatorUI : MoveCalculatorUIBase
	{
		public override void Init()
		{
			moveType = MoveType.ApproachToEnemy;

			CalculatorType = typeof(ApproachToEnemyMoveCalculator);

			base.Init();
		}
	}
}