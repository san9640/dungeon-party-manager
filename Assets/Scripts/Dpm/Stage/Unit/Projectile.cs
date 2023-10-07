using System;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Render;
using Dpm.Stage.Spec;
using Dpm.Stage.Unit.State;
using Dpm.Utility.Extensions;
using Dpm.Utility.Pool;
using Dpm.Utility.State;
using UnityEngine;
using CustomCollider2D = Dpm.Stage.Physics.CustomCollider2D;

namespace Dpm.Stage.Unit
{
	[RequireComponent(typeof(CustomCollider2D))]
	public class Projectile : MonoBehaviour, IProjectile
	{
		[SerializeField]
		private SpriteAnimator animator;

		public Vector2 LookDir
		{
			set
			{
				var v3Dir = value.ConvertToVector3();
				var rotation = Quaternion.FromToRotation(Vector3.right, v3Dir);

				animator.Rotation = rotation.eulerAngles.z;
			}
		}

		public Vector2 Position {
			get => Bounds.center;
			set
			{
				var bounds = Bounds;
				bounds.center = value;

				Bounds = bounds;

				transform.position = value.ConvertToVector3();
			}
		}

		public Bounds2D Bounds { get; private set; }

		private readonly StateMachine _stateMachine = new();

		public IState CurrentState => _stateMachine.CurrentState;

		public IUnit Shooter { get; private set; }

		public float Speed => Spec.speed;

		public ProjectileSpec Spec { get; set; }

		public void Dispose()
		{
			_stateMachine.ChangeState(null);
			Shooter = null;
		}

		public bool OnSimulateCrash(ICustomCollider other)
		{
			if (other is IUnit unit)
			{
				return Shooter.Region.IsOppositeParty(unit.Region);
			}

			return false;
		}

		public void OnEvent(Core.Interface.Event e)
		{
			// if (e is StateChangeEvent sce)
			// {
			// 	_stateMachine.ChangeState(sce.State);
			// }

			if (e is CrashedEvent ce)
			{
				// 데미지 이벤트 전송
				CoreService.Event.SendImmediate(ce.Crasher,
					DamageEvent.Create(Shooter, Spec.defaultDamage));

				GameObjectPool.Get(Spec.hitFxSpecName).TrySpawn(Position.ConvertToVector3(), out _);

				ProjectileManager.Instance.Despawn(this);
			}
			else if (e is RequestShootEvent rse)
			{
				Shooter = rse.Shooter;

				IState moveState = Spec.type switch
				{
					ProjectileType.Follow => ProjectileFollowState.Create(this, rse.Target),
					ProjectileType.Linear => ProjectileMoveState.Create(this, rse.Target),
					_ => null
				};

				if (moveState != null)
				{
					_stateMachine.ChangeState(moveState);
				}
				else
				{
					ProjectileManager.Instance.Despawn(this);
				}
			}
		}
	}
}