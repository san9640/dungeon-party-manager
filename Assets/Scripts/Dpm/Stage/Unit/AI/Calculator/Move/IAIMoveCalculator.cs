using Dpm.Stage.Spec;
using UnityEngine;

namespace Dpm.Stage.Unit.AI.Calculator.Move
{
	public interface IAIMoveCalculator : IAICalculator
	{
		void Init(Character character, MoveCalculatorInfo info);

		Vector2? TargetPos { get; }
	}
}