using Dpm.Stage.Spec;
using Dpm.Stage.Unit.AI.Calculator.Move;

namespace Dpm.Stage.UI.Calculator.Move
{
	public class AwayFromWallMoveCalculatorUI : MoveCalculatorUIBase
	{
		public override void Init()
		{
			moveType = MoveType.AwayFromWall;

			CalculatorType = typeof(AwayFromWallMoveCalculator);

			base.Init();
		}
	}
}