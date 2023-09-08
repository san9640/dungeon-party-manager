using System;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Unit.State;
using Dpm.Utility.State;
using UnityEngine;

namespace Dpm.Stage.Unit
{
	[RequireComponent(typeof(UnitSpec))]
	public class Unit : MonoBehaviour, IUnit
	{
		public string Name { get; protected set; }

		public int Id { get; protected set; }

		public UnitRegion Region { get; protected set; } = UnitRegion.None;

		public Vector2 Position
		{
			get => Bounds.center;
			set
			{
				var bounds = Bounds;
				bounds.center = value;

				Bounds = bounds;

				transform.position = new Vector3(value.x, value.y, 0);
			}
		}

		public Bounds2D Bounds { get; protected set; }

		public IState CurrentState => _stateMachine.CurrentState;

		protected StateMachine _stateMachine = new();

		private void Awake()
		{
			OnInit();
		}

		public virtual void OnInit()
		{
			var spec = GetComponent<UnitSpec>();

			Bounds = new Bounds2D(transform.position, spec.boundsSize);

			Region = UnitRegion.Neutral;
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
	}
}