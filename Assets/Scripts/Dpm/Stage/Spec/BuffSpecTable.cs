using System;
using Dpm.Stage.Buff;
using Dpm.Utility;
using UnityEngine;

namespace Dpm.Stage.Spec
{
	[Serializable]
	public struct BuffSpec : IGameSpec
	{
		public string name;

		public string Name => name;

		public BuffType type;

		public float value;

		public int IntValue => Mathf.RoundToInt(value);
	}

	[CreateAssetMenu(menuName = "StageSpecs/Buff", fileName = "BuffSpecTable")]
	public class BuffSpecTable : SpecTableBase<BuffSpec>, IGameSpecTable
	{
	}
}