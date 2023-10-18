using System;
using Dpm.Stage.Unit;
using Dpm.Utility;
using UnityEngine;

namespace Dpm.Stage.Spec
{
	[Serializable]
	public struct PartySpawnInfo
	{
		public string characterSpecName;

		// TODO : 엘리트 옵션 등 추가
		public Vector2Int spawnIndex;
	}

	[Serializable]
	public struct PartySpec : IGameSpec
	{
		public string name;

		public string Name => name;

		public UnitRegion region;

		public PartySpawnInfo[] spawnInfos;
	}

	[CreateAssetMenu(menuName = "StageSpecs/Party", fileName = "PartySpecTable")]
	public class PartySpecTable : SpecTableBase<PartySpec>, IGameSpecTable
	{
	}
}