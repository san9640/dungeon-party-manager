using System;
using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage.Physics;
using Dpm.Stage.Render;
using UnityEngine;

namespace Dpm.Stage.Unit
{
	public class Character : ObjectUnit, IUpdatable, IEventListener, IDisposable
	{
		public SpriteAnimator Animator { get; private set; }

		private void Awake()
		{
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