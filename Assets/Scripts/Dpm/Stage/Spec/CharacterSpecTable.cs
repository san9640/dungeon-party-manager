using System;
using Dpm.Utility;
using UnityEngine;

namespace Dpm.Stage.Spec
{
	[Serializable]
	public struct CharacterSpec : IGameSpec
	{
		public string name;

		public string Name => name;

		public string poolSpecName;

		public int baseHp;

		public int baseMp;

		public float baseDamageFactor;

		public float baseAttackSpeed;

		public float moveSpeed;

		public string battleActionSpecName;

		public string moveSpecName;

		public string attackSpecName;

		public string specialSpecName;
	}

	[CreateAssetMenu(menuName = "StageSpecs/Character", fileName = "CharacterSpecTable")]
	public class CharacterSpecTable : SpecTableBase<CharacterSpec>
	{
	}
}