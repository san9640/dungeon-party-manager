using System;
using System.Collections.Generic;
using Dpm.Utility;
using UnityEngine;

namespace Dpm.Stage.Spec
{
	public enum MoveType
	{
		ApproachToEnemy,
		Retreat,
		AwayFromWall,
		DistanceFromAllies,
	}

	[Serializable]
	public struct MoveCalculatorInfo
	{
		public MoveType type;
		public SliderWeightFactorInfo weightFactorInfo;
	}

	[Serializable]
	public struct MoveSpec : IGameSpec
	{
		public string name;

		public string Name => name;

		public List<MoveCalculatorInfo> calculatorInfos;
	}

	[CreateAssetMenu(menuName = "StageSpecs/Move", fileName = "MoveSpecTable")]
	public class MoveSpecTable : SpecTableBase<MoveSpec>, IGameSpecTable
	{
	}
}