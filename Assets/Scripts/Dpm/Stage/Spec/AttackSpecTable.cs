using System;
using System.Collections.Generic;
using Dpm.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dpm.Stage.Spec
{
	public enum AttackTargetSearchingType
	{
		Closest,
		Weakest,
		Strongest,
		LowHp,
		HighHp,
		Melee,
		Ranged,
	}

	[Serializable]
	public struct AttackCalculatorInfo
	{
		public AttackTargetSearchingType type;
		public SliderWeightFactorInfo weightFactorInfo;
	}

	[Serializable]
	public struct AttackSpec : IGameSpec
	{
		public string name;

		public string Name => name;

		public List<AttackCalculatorInfo> calculatorInfos;
	}

	[CreateAssetMenu(menuName = "StageSpecs/Attack", fileName = "AttackSpecTable")]
	public class AttackSpecTable : SpecTableBase<AttackSpec>, IGameSpecTable
	{
	}
}