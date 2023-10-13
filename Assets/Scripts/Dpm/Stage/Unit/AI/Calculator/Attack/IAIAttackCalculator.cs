using Dpm.Stage.Spec;

namespace Dpm.Stage.Unit.AI.Calculator.Attack
{
	public interface IAIAttackCalculator : IAICalculator
	{
		void Init(Character character, AttackCalculatorInfo info);

		IUnit CurrentTarget { get; }
	}
}