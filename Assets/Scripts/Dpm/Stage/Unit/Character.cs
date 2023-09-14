using System;
using Dpm.Stage.Event;
using Dpm.Stage.Render;
using Dpm.Stage.Unit.State;
using Dpm.Utility.Constants;
using UnityEngine;

namespace Dpm.Stage.Unit
{
	public class Character : Unit, IDisposable
	{
		[SerializeField]
		private SpriteAnimator animator;

		public SpriteAnimator Animator => animator;

		public Direction Direction
		{
			get => Animator.LookDirection;
			set => Animator.LookDirection = value;
		}

		private void Awake()
		{
			OnInit();
		}

		public void Dispose()
		{
		}

		public override void OnEvent(Core.Interface.Event e)
		{
			if (e is UnitRegisteredEvent)
			{
				_stateMachine.ChangeState(CharacterWaitBattleState.Create(this));

				// Unit.OnEvent에서 해당 이벤트에 대한 처리를 하지 않도록 리턴
				return;
			}
			else if (e is UnitUnregisteredEvent)
			{
				_stateMachine.ChangeState(CharacterWaitBattleState.Create(null));

				// Unit.OnEvent에서 해당 이벤트에 대한 처리를 하지 않도록 리턴
				return;
			}

			base.OnEvent(e);
		}
	}
}