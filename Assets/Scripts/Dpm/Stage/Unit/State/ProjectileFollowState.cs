using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Utility.State;

namespace Dpm.Stage.Unit.State
{
	public class ProjectileFollowState : PooledState<ProjectileFollowState>, IUpdatable
	{
		private IProjectile _projectile;
		private IUnit _target;

		public override void Enter()
		{
			_projectile.LookDir = (_target.Position - _projectile.Position).normalized;

			// 업데이트 순서는 Shooter의 ID를 기준으로 하지만, 실제 업데이트 순서는 Shooter보다 늦다.
			// Shooter의 업데이트는 미리 등록되어있기 때문.
			CoreService.FrameUpdate.RegisterUpdate(this, _projectile.Shooter.Id);

			CoreService.Event.PublishImmediate(AddToPartitionEvent.Create(_projectile));
		}

		public override void Exit()
		{
			CoreService.Event.PublishImmediate(RemoveFromPartitionEvent.Create(_projectile));

			CoreService.FrameUpdate.UnregisterUpdate(this, _projectile.Shooter.Id);
		}

		public override void Dispose()
		{
			_projectile = null;

			base.Dispose();
		}

		public void UpdateFrame(float dt)
		{
			var moveDir = (_target.Position - _projectile.Position).normalized;
			var dist = _projectile.Speed * dt;

			_projectile.LookDir = moveDir;

			StagePhysicsManager.Instance.Move(_projectile, moveDir, dist);
		}

		public static ProjectileFollowState Create(IProjectile projectile, IUnit target)
		{
			var state = Pool.GetOrCreate();

			state._projectile = projectile;
			state._target = target;

			return state;
		}
	}
}