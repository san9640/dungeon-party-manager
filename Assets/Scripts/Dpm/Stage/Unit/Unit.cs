using System;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
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

		public virtual void EnterField()
		{
			CoreService.Event.PublishImmediate(AddedToPartitionEvent.Create(this));
		}

		public virtual void ExitField()
		{
			CoreService.Event.PublishImmediate(RemovedFromPartitionEvent.Create(this));
		}
	}
}