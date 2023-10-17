using System;
using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Render;
using Dpm.Stage.Spec;
using Dpm.Stage.Unit.AI;
using Dpm.Stage.Unit.Battle;
using Dpm.Stage.Unit.Battle.BattleAction;
using Dpm.Stage.Unit.State;
using Dpm.Utility.Constants;
using Dpm.Utility.Extensions;
using UnityEditor;
using UnityEngine;
using CustomCollider2D = Dpm.Stage.Physics.CustomCollider2D;

namespace Dpm.Stage.Unit
{
	[RequireComponent(typeof(CustomCollider2D))]
	public class Character : Unit, IDisposable, IUpdatable
	{
		[SerializeField]
		private SpriteAnimator animator;

		public SpriteAnimator Animator => animator;

		public Direction Direction
		{
			get => Animator.LookDirection;
			set => Animator.LookDirection = value;
		}

		public IDecisionMaker DecisionMaker { get; } = new DecisionMaker();

		public IBattleAction BattleAction { get; private set; }

		public int Hp { get; private set; }

		public int MaxHp { get; private set; }

		public float HpRatio => Hp / (float)MaxHp;

		public bool IsDead => Hp == 0;

		public int Mp { get; private set; }

		public int MaxMp { get; private set; }

		public float DamageFactor { get; private set; }

		public int AttackDamage => Mathf.FloorToInt(DamageFactor * BattleAction.Spec.damage);

		public float AttackSpeed { get; private set; }

		public float MoveSpeed { get; private set; }

		private void Awake()
		{
			var boundsHolder = GetComponent<CustomCollider2D>();

			Bounds = boundsHolder.bounds;
			Position = transform.position;
		}

		public void Dispose()
		{
			DecisionMaker.Dispose();

			BattleAction?.Dispose();
			BattleAction = null;
		}

		public void Init(CharacterSpec spec)
		{
			MaxHp = spec.baseHp;
			Hp = MaxHp;

			MaxMp = spec.baseMp;
			Mp = MaxMp;

			DamageFactor = spec.baseDamageFactor;

			AttackSpeed = spec.baseAttackSpeed;

			MoveSpeed = spec.moveSpeed;

			var battleActionSpec = SpecUtility.GetSpec<BattleActionSpec>(spec.battleActionSpecName);

			BattleAction = battleActionSpec.type switch
			{
				BattleActionAttackType.Melee => new MeleeBattleAction(),
				BattleActionAttackType.Ranged => new RangedShooterBattleAction(),
				_ => null
			};

			BattleAction?.Init(this, battleActionSpec);

			var moveSpec = SpecUtility.GetSpec<MoveSpec>(spec.moveSpecName);
			var attackSpec = SpecUtility.GetSpec<AttackSpec>(spec.attackSpecName);

			DecisionMaker.Init(this, moveSpec, attackSpec);
		}

		public override void OnEvent(Core.Interface.Event e)
		{
			if (CurrentState is IEventListener listener)
			{
				listener.OnEvent(e);
			}

			if (e is UnitRegisteredEvent)
			{
				_stateMachine.ChangeState(CharacterWaitBattleState.Create(this));

				CoreService.FrameUpdate.RegisterUpdate(this, Id);

				// Unit.OnEvent에서 해당 이벤트에 대한 처리를 하지 않도록 리턴
				return;
			}
			else if (e is UnitUnregisteredEvent)
			{
				CoreService.FrameUpdate.UnregisterUpdate(this, Id);

				_stateMachine.ChangeState(null);

				// Unit.OnEvent에서 해당 이벤트에 대한 처리를 하지 않도록 리턴
				return;
			}
			else if (e is DamageEvent de)
			{
				if (CurrentState is CharacterBattleState && !IsDead)
				{
					Hp = Mathf.Max(Hp - de.Damage, 0);

					CoreService.Event.Publish(HpChangedEvent.Create(this));

					if (IsDead)
					{
						_stateMachine.ChangeState(CharacterDeadState.Create(this));
					}
				}
			}

			base.OnEvent(e);
		}

		public void UpdateFrame(float dt)
		{
			(CurrentState as IUpdatable)?.UpdateFrame(dt);
		}

		public void OnDrawGizmos()
		{
			var style = new GUIStyle
			{
				normal =
				{
					textColor = Color.red
				},
				fontSize = 20,
			};

			// Handles.Label(screenPosition, $"[{Id}]", style);
			Handles.Label(Position.ConvertToVector3(), $"[{Id}]", style);

			if (CurrentState is CharacterBattleState)
			{
				Handles.color = Region == UnitRegion.Ally ? Color.cyan : Color.yellow;
				Handles.DrawWireDisc(Position.ConvertToVector3(), Vector3.forward, BattleAction.Spec.attackRange);

				(DecisionMaker as IDebugDrawable)?.DrawCurrent();
			}
		}
	}
}