using Dpm.Stage.Spec;
using Dpm.Stage.Unit.AI.Calculator.Move;

namespace Dpm.Stage.UI.Calculator.Move
{
	public class RetreatMoveCalculatorUI : MoveCalculatorUIBase
	{
		public override void Init()
		{
			moveType = MoveType.Retreat;

			CalculatorType = typeof(RetreatMoveCalculator);

			base.Init();
		}
	}
}