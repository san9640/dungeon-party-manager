using System.Collections.Generic;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Spec;
using Dpm.Utility.Pool;
using UnityEngine;

namespace Dpm.Stage.Unit
{
	public class ProjectileManager
	{
		public static ProjectileManager Instance => StageScene.Instance.ProjectileManager;

		private readonly Dictionary<IProjectile, PooledGameObject> _usingProjectiles = new();

		private ProjectileSpecsHolder SpecsHolder
		{
			get
			{
				var holder = CoreService.Asset.UnsafeGet<ScriptableObject>("ProjectileSpec");

				return holder as ProjectileSpecsHolder;
			}
		}

		public IProjectile Spawn(string projectileSpecName)
		{
			if (!SpecsHolder.NameToSpec.TryGetValue(projectileSpecName, out var spec))
			{
				return null;
			}

			var projectilePool = GameObjectPool.Get(spec.bodySpecName);

			if (!projectilePool.TrySpawn(Vector3.zero, out var projectileGo))
			{
				return null;
			}

			var projectile = projectileGo.GetComponent<Projectile>();

			if (!_usingProjectiles.ContainsKey(projectile))
			{
				_usingProjectiles.Add(projectile, projectileGo);
			}

			projectile.Spec = spec;

			return projectile;
		}

		public void Shoot(string projectileSpecName, IUnit shooter, IUnit target)
		{
			var projectile = Spawn(projectileSpecName);

			var toTargetDir = (target.Position - shooter.Position).normalized;

			projectile.Position = shooter.Position + toTargetDir * 0.7f;

			CoreService.Event.SendImmediate(projectile, RequestShootEvent.Create(shooter, target));
		}

		public void Despawn(IProjectile projectile)
		{
			if (!_usingProjectiles.TryGetValue(projectile, out var projectileGo))
			{
				return;
			}

			_usingProjectiles.Remove(projectile);

			projectile.Dispose();
			projectileGo.Deactivate();
		}
	}
}