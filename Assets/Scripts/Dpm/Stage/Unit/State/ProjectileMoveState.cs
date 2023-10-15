using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Utility.State;
using UnityEngine;

namespace Dpm.Stage.Unit.State
{
	public class ProjectileMoveState : PooledState<ProjectileMoveState>, IUpdatable
	{
		private IProjectile _projectile;
		private Vector2 _targetPos;
		private Vector2 _moveDir;

		public override void Enter()
		{
			_moveDir = (_targetPos - _projectile.Position).normalized;

			_projectile.LookDir = _moveDir;

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
			var dist = _projectile.Speed * dt;

			// 발사 위치가 특정 유닛과 겹쳐있을 가능성이 존재하여 겹쳐있을 경우에도 충돌 검사가 되도록 함
			StagePhysicsManager.Instance.Move(_projectile, _moveDir, dist, CrashOption.CrashOnOverlap);
		}

		public static ProjectileMoveState Create(IProjectile projectile, Vector2 targetPos)
		{
			var state = Pool.GetOrCreate();

			state._projectile = projectile;
			state._targetPos = targetPos;

			return state;
		}

		public static ProjectileMoveState Create(IProjectile projectile, IUnit target)
		{
			var state = Pool.GetOrCreate();

			state._projectile = projectile;
			state._targetPos = target.Position;

			return state;
		}
	}
}