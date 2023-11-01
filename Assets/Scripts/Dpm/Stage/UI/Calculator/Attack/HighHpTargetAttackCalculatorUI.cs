using Dpm.Stage.Spec;
using Dpm.Stage.Unit.AI.Calculator.Attack;

namespace Dpm.Stage.UI.Calculator.Attack
{
	public class HighHpTargetAttackCalculatorUI : AttackCalculatorUIBase
	{
		public override void Init()
		{
			targetSearchingType = AttackTargetSearchingType.HighHp;

			CalculatorType = typeof(HighHpTargetAttackCalculator);

			base.Init();
		}
	}
}