using Core.Interface;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Spec;
using UnityEngine;

namespace Dpm.Stage.Unit.Battle.BattleAction
{
	public class RangedShooterBattleAction : IBattleAction, IUpdatable
	{
		public BattleActionSpec Spec { get; private set; }

		private Character _character;

		private float _timePassed;

		private enum State
		{
			Idle,
			Cooling
		}

		private State _currentState = State.Idle;

		public float Dps
		{
			get
			{
				var totalDelay = Spec.attackDelay / _character.AttackSpeed;
				var totalDamage = _character.AttackDamage;

				return totalDamage / totalDelay;
			}
		}

		public void Init(Character character, BattleActionSpec spec)
		{
			Spec = spec;
			_character = character;

			_currentState = State.Idle;
		}

		public void Dispose()
		{
			_character = null;
		}

		public void Reset()
		{
			_currentState = State.Idle;
			_timePassed = 0;
		}

		public void OnEvent(Core.Interface.Event e)
		{
			if (e is RequestAttackTargetEvent rae)
			{
				if (_currentState != State.Idle)
				{
					return;
				}

				if (PhysicsUtility.GetDistanceBtwCollider(_character, rae.Target) > Spec.attackRange)
				{
					return;
				}

				_currentState = State.Cooling;

				ProjectileManager.Instance.Shoot(Spec.rangedProjectileName, new ProjectileInfo
				{
					shooter = _character,
					damage = _character.AttackDamage,
					target = rae.Target
				});
			}
		}

		public void UpdateFrame(float dt)
		{
			if (_currentState == State.Cooling)
			{
				_timePassed += dt;

				if (_timePassed >= Spec.attackDelay / _character.AttackSpeed)
				{
					_timePassed = 0;
					_currentState = State.Idle;
				}
			}
		}
	}
}