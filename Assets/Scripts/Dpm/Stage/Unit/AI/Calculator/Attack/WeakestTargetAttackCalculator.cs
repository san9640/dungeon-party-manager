using Dpm.Stage.Spec;
using UnityEngine;

namespace Dpm.Stage.Unit.AI.Calculator.Attack
{
	public class WeakestTargetAttackCalculator : IAIAttackCalculator
	{
		private AttackCalculatorInfo _info;

		private Character _character;

		public IUnit CurrentTarget { get; private set; }

		private float _attackableDist;

		public void Init(Character character, AttackCalculatorInfo info)
		{
			_character = character;
			_info = info;
			CurrentTarget = null;

			_attackableDist = Mathf.Max(
				_character.BattleAction.Spec.attackRange - AICalculatorConstants.AttackDistEpsilon, 0f);
		}

		public void Dispose()
		{
			_character = null;
			CurrentTarget = null;
		}

		public float Calculate()
		{
			return 0;
		}

		public void Execute()
		{

		}
		public void DrawCurrent()
		{
		}
	}
}