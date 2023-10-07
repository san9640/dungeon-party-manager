using System;
using System.Collections.Generic;
using System.Linq;
using Dpm.Stage.Unit;
using Dpm.Utility;
using UnityEngine;

namespace Dpm.Stage.Spec
{
	[Serializable]
	public struct ProjectileSpec : IGameSpec
	{
		[SerializeField]
		private string name;

		public string Name => name;

		public string bodySpecName;
		public float speed;
		public ProjectileType type;
		public string hitFxSpecName;
		public int defaultDamage;
	}

	[CreateAssetMenu(menuName = "StageSpecs/Projectile", fileName = "ProjectileSpecsHolder")]
	public class ProjectileSpecsHolder :  SpecsHolderBase<ProjectileSpec>
	{
	}
}