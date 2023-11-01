using Dpm.Stage.Spec;
using Dpm.Stage.Unit.AI.Calculator.Attack;

namespace Dpm.Stage.UI.Calculator.Attack
{
	public class RangedTargetAttackCalculatorUI : AttackCalculatorUIBase
	{
		public override void Init()
		{
			targetSearchingType = AttackTargetSearchingType.Ranged;

			CalculatorType = typeof(RangedTargetAttackCalculator);

			base.Init();
		}
	}
}