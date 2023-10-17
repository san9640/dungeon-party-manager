using System;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Unit.State;
using Dpm.Utility.Extensions;
using Dpm.Utility.State;
using UnityEngine;
using CustomCollider2D = Dpm.Stage.Physics.CustomCollider2D;

namespace Dpm.Stage.Unit
{
	public class Unit : MonoBehaviour, IUnit
	{
		public string Name
		{
			get => gameObject.name;
			set => gameObject.name = value;
		}

		public int Id { get; set; }

		public UnitRegion Region { get; set; } = UnitRegion.None;

		public Vector2 Position
		{
			get => Bounds.center;
			set
			{
				var bounds = Bounds;
				bounds.center = value;

				Bounds = bounds;

				transform.position = value.ConvertToVector3();
			}
		}

		public Bounds2D Bounds { get; protected set; }

		public IState CurrentState => _stateMachine.CurrentState;

		protected StateMachine _stateMachine = new();

		private void Awake()
		{
			var boundsHolder = GetComponent<CustomCollider2D>();

			Bounds = boundsHolder.bounds;
			Position = transform.position;
		}

		public bool OnSimulateCrash(ICustomCollider other)
		{
			return other switch
			{
				IUnit => true,
				IProjectile projectile => Region.IsOppositeParty(projectile.Shooter.Region),
				_ => false
			};
		}

		public virtual void OnEvent(Core.Interface.Event e)
		{
			if (e is StateChangeEvent sce)
			{
				_stateMachine.ChangeState(sce.State);
			}
			else if (e is UnitRegisteredEvent)
			{
				_stateMachine.ChangeState(UnitIdleState.Create(this));
			}
			else if (e is UnitUnregisteredEvent)
			{
				_stateMachine.ChangeState(null);
			}
		}
		//
		// public void OnDrawGizmos()
		// {
		// 	Gizmos.color = Color.magenta;
		//
		// 	Gizmos.DrawCube(Bounds.center.ConvertToVector3(), Bounds.Size.ConvertToVector3());
		// }
	}
}