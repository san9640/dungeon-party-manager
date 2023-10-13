using Dpm.Stage.Spec;

namespace Dpm.Stage.Unit.AI.Calculator.Move
{
	public interface IAIMoveCalculator : IAICalculator
	{
		void Init(Character character, MoveCalculatorInfo info);
	}
}