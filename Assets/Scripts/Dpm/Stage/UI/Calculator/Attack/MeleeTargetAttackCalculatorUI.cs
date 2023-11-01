using Dpm.Stage.Spec;
using Dpm.Stage.Unit.AI.Calculator.Attack;

namespace Dpm.Stage.UI.Calculator.Attack
{
	public class MeleeTargetAttackCalculatorUI : AttackCalculatorUIBase
	{
		public override void Init()
		{
			targetSearchingType = AttackTargetSearchingType.Melee;

			CalculatorType = typeof(MeleeTargetAttackCalculator);

			base.Init();
		}
	}
}