using Dpm.Stage.Spec;
using Dpm.Stage.Unit.AI.Calculator.Attack;

namespace Dpm.Stage.UI.Calculator.Attack
{
	public class LowHpTargetAttackCalculatorUI : AttackCalculatorUIBase
	{
		public override void Init()
		{
			targetSearchingType = AttackTargetSearchingType.LowHp;

			CalculatorType = typeof(LowHpTargetAttackCalculator);

			base.Init();
		}
	}
}