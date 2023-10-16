using Dpm.CoreAdapter;
using Dpm.Stage.Spec;
using Dpm.Stage.Unit.AI.Calculator.Attack;

namespace Dpm.Stage.UI.Calculator.Attack
{
	public class ClosestTargetAttackCalculatorUI : AttackCalculatorUIBase
	{
		public override void Init()
		{
			targetSearchingType = AttackTargetSearchingType.Closest;

			CalculatorType = typeof(ClosestTargetAttackCalculator);

			base.Init();
		}
	}
}