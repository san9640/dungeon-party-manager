using System;
using Dpm.Utility;
using UnityEngine;

namespace Dpm.Stage.Spec
{
	public enum BattleActionAttackType
	{
		Melee,
		Ranged,
	}

	[Serializable]
	public struct BattleActionSpec : IGameSpec
	{
		public string name;

		public string Name => name;

		public float attackRange;

		public float attackDelay;

		public float damage;

		public BattleActionAttackType type;

		public string meleeFx;

		public string meleeHitFx;

		public float meleeDuration;

		public float meleeAttackAngle;

		public string rangedProjectileName;

		public float rangedProjectileSpeed;
	}

	[CreateAssetMenu(menuName = "StageSpecs/BattleAction", fileName = "BattleActionSpecTable")]
	public class BattleActionSpecTable : SpecTableBase<BattleActionSpec>, IGameSpecTable
	{
	}
}