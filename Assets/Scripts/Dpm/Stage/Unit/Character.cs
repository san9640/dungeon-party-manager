using System;
using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage.Field;
using Dpm.Stage.Physics;
using Dpm.Stage.Render;
using Dpm.Stage.Unit.AI;
using Dpm.Utility;
using UnityEngine;

namespace Dpm.Stage.Unit
{
	public class Character : MonoBehaviour, IUnit, IUpdatable, IEventListener, IDisposable
	{
		public int EntityId { get; set; }

		public int Name { get; set; }

		public Vector2 Position
		{
			get => new Vector2(transform.position.x, transform.position.y);
			set => transform.position = new Vector3(value.x, value.y, 0);
		}

		// public Bound2D Bounds => new(new Vector2(Position.x, Position.y), spec.BoundSize);

		// public CharacterSpec spec;

		public SpriteAnimator Animator { get; private set; }

		private void Awake()
		{
			Animator = GetComponent<SpriteAnimator>();
		}

		public void Init()
		{
		}

		public void Dispose()
		{
		}

		public void EnterField()
		{
			CoreService.FrameUpdate.RegisterUpdate(this, EntityId);
		}

		public void ExitField()
		{
			CoreService.FrameUpdate.UnregisterUpdate(this, EntityId);
		}

		public void UpdateFrame(float dt)
		{
			var dist = dt * 1;
			var xMove = Input.GetAxis("Horizontal");
			var yMove = Input.GetAxis("Vertical");

			var moveDir = new Vector2(xMove, yMove).normalized;

			Position += moveDir * dist;
		}

		public void OnEvent(Core.Interface.Event e)
		{

		}

		private void UpdateAI(float dt)
		{

		}
	}
}