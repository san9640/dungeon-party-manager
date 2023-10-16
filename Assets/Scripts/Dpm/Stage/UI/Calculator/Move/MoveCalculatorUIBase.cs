using Dpm.Stage.Spec;
using Dpm.Stage.Unit.AI;

namespace Dpm.Stage.UI.Calculator.Move
{
	public class MoveCalculatorUIBase : AICalculatorUIBase
	{
		protected MoveType moveType;

		public MoveType MoveType => moveType;

		public override void Init()
		{
			aiType = AICalculatorType.Move;

			base.Init();
		}
	}
}