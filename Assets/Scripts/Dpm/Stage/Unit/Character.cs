using System;
using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage.Buff;
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

		public Vector2 OriginPos { get; private set; }

		public Direction OriginDir { get; private set; }

		public IDecisionMaker DecisionMaker { get; } = new DecisionMaker();

		public IBattleAction BattleAction { get; private set; }

		public int Hp { get; private set; }

		private readonly BuffAddCalculator _maxHpBuffCalculator = new();

		public int MaxHp => _originMaxHp + _maxHpBuffCalculator.Value;

		private int _originMaxHp;

		public float HpRatio => Hp / (float)MaxHp;

		public bool IsDead => Hp == 0;

		public int Mp { get; private set; }

		public int MaxMp { get; private set; }

		private float _originDamageFactor;

		private readonly BuffMultiplyCalculator _damageBuffCalculator = new();

		private float DamageFactor => _originDamageFactor * _damageBuffCalculator.Value;

		public int AttackDamage => Mathf.FloorToInt(DamageFactor * BattleAction.Spec.damage);

		private float _originAttackSpeed;

		private readonly BuffMultiplyCalculator _attackSpeedBuffCalculator = new();

		public float AttackSpeed => _originAttackSpeed * _attackSpeedBuffCalculator.Value;

		public float MoveSpeed { get; private set; }

		private void Awake()
		{
			var boundsHolder = GetComponent<CustomCollider2D>();

			Bounds = boundsHolder.bounds;
			Position = transform.position.ConvertToVector2();
		}

		public void Dispose()
		{
			DecisionMaker.Dispose();

			BattleAction?.Dispose();
			BattleAction = null;

			_maxHpBuffCalculator.Dispose();
			_damageBuffCalculator.Dispose();
			_attackSpeedBuffCalculator.Dispose();
		}

		public void Init(CharacterSpec spec)
		{
			_originMaxHp = spec.baseHp;
			Hp = _originMaxHp;

			MaxMp = spec.baseMp;
			Mp = MaxMp;

			_originDamageFactor = spec.baseDamageFactor;

			_originAttackSpeed = spec.baseAttackSpeed;

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
				OriginPos = Position;
				OriginDir = Direction;

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
			else if (e is HealEvent he)
			{
				var wasDead = IsDead;

				Hp = Mathf.Min(Hp + he.Value, MaxHp);

				CoreService.Event.Publish(HpChangedEvent.Create(this));

				if (wasDead != IsDead && CurrentState is CharacterDeadState)
				{
					_stateMachine.ChangeState(CharacterAfterBattleState.Create(this));
				}
			}
			else if (e is AddAttackSpeedBuffEvent asbe)
			{
				if (asbe.IsPermanent)
				{
					_attackSpeedBuffCalculator.AddPermanentBuff(asbe.Value);
				}
				else
				{
					_attackSpeedBuffCalculator.AddBuff(asbe.Key, asbe.Value);
				}

				CoreService.Event.PublishImmediate(AttackSpeedChangedEvent.Create(this));
			}
			else if (e is AddDamageBuffEvent adbe)
			{
				if (adbe.IsPermanent)
				{
					_damageBuffCalculator.AddPermanentBuff(adbe.Value);
				}
				else
				{
					_damageBuffCalculator.AddBuff(adbe.Key, adbe.Value);
				}

				CoreService.Event.PublishImmediate(AttackDamageChangedEvent.Create(this));
			}
			else if (e is AddMaxHpBuffEvent ahbe)
			{
				if (ahbe.IsPermanent)
				{
					_maxHpBuffCalculator.AddPermanentBuff(ahbe.Value);
				}
				else
				{
					_maxHpBuffCalculator.AddBuff(ahbe.Key, ahbe.Value);
				}

				CoreService.Event.PublishImmediate(MaxHpChangedEvent.Create(this));
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