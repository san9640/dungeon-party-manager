using System.Collections.Generic;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Spec;
using Dpm.Utility.Pool;
using UnityEngine;

namespace Dpm.Stage.Unit
{
	public struct ProjectileInfo
	{
		public int damage;

		public IUnit shooter;

		public IUnit target;

		public Vector2 targetPos;
	}

	public class ProjectileManager
	{

		public static ProjectileManager Instance => StageScene.Instance.ProjectileManager;

		private readonly Dictionary<IProjectile, PooledGameObject> _usingProjectiles = new();

		private ProjectileSpecTable SpecTable
		{
			get
			{
				var table = CoreService.Asset.UnsafeGet<ScriptableObject>("ProjectileSpecTable");

				return table as ProjectileSpecTable;
			}
		}

		private IProjectile Spawn(string projectileSpecName)
		{
			if (!SpecTable.NameToSpec.TryGetValue(projectileSpecName, out var spec))
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

		public void Shoot(string projectileSpecName, ProjectileInfo info)
		{
			var projectile = Spawn(projectileSpecName);

			if (info.target != null)
			{
				info.targetPos = info.target.Position;
			}

			var toTargetDir = (info.targetPos - info.shooter.Position).normalized;

			projectile.Position = info.shooter.Position + toTargetDir * 0.7f;

			CoreService.Event.SendImmediate(projectile, RequestMoveProjectileEvent.Create(info));
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

		public void Clear()
		{
			foreach (var kv in _usingProjectiles)
			{
				kv.Key.Dispose();
				kv.Value.Deactivate();
			}
		}
	}
}