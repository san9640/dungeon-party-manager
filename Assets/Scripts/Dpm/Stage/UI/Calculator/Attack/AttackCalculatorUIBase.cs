using System;
using Dpm.Stage.Spec;
using Dpm.Stage.Unit.AI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dpm.Stage.UI.Calculator.Attack
{
	public class AttackCalculatorUIBase : AICalculatorUIBase
	{
		protected AttackTargetSearchingType targetSearchingType;

		public AttackTargetSearchingType TargetSearchingType => targetSearchingType;

		public override void Init()
		{
			aiType = AICalculatorType.Attack;

			base.Init();
		}
	}
}