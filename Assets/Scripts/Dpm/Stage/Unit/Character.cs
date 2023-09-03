using System;
using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage.Physics;
using Dpm.Stage.Render;
using Dpm.Utility.Constants;
using UnityEngine;

namespace Dpm.Stage.Unit
{
	public class Character : Unit, IUpdatable, IEventListener, IDisposable
	{
		public SpriteAnimator Animator { get; private set; }

		public Direction Direction
		{
			get => Animator.LookDirection;
			set => Animator.LookDirection = value;
		}

		private void Awake()
		{
			OnInit();
		}

		public override void OnInit()
		{
			base.OnInit();

			Animator = GetComponent<SpriteAnimator>();
		}

		public void Dispose()
		{
		}

		public override void EnterField()
		{
			base.EnterField();

			CoreService.FrameUpdate.RegisterUpdate(this, Id);
		}

		public override void ExitField()
		{
			CoreService.FrameUpdate.UnregisterUpdate(this, Id);

			base.ExitField();
		}

		public void UpdateFrame(float dt)
		{
			var dist = dt * 3;
			var xMove = Input.GetAxis("Horizontal");
			var yMove = Input.GetAxis("Vertical");

			var moveDir = new Vector2(xMove, yMove).normalized;

			StagePhysicsManager.Instance.MoveUnit(this, moveDir, dist);
		}

		public void OnEvent(Core.Interface.Event e)
		{

		}

		private void UpdateAI(float dt)
		{

		}
	}
}