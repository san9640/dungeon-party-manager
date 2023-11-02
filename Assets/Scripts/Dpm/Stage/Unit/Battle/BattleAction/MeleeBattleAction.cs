using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Physics;
using Dpm.Stage.Spec;
using Dpm.Utility.Extensions;
using Dpm.Utility.Pool;
using UnityEngine;

namespace Dpm.Stage.Unit.Battle.BattleAction
{
	public class MeleeBattleAction : IBattleAction, IUpdatable
	{
		private Character _character;
		public BattleActionSpec Spec { get; private set; }

		private float _timePassed;

		private enum State
		{
			Idle,
			Attacking,
			Cooling,
		}

		private State _currentState = State.Idle;

		public float Dps
		{
			get
			{
				var totalDelay = (Spec.meleeDuration + Spec.attackDelay) / _character.AttackSpeed;
				var totalDamage = _character.AttackDamage;

				return totalDamage / totalDelay;
			}
		}

		public void Init(Character character, BattleActionSpec spec)
		{
			_character = character;
			Spec = spec;
			_currentState = State.Idle;
		}

		public void Reset()
		{
			_currentState = State.Idle;
			_timePassed = 0;
		}

		public void Dispose()
		{
			_character = null;
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

				_currentState = State.Attacking;

				var hitFxPool = GameObjectPool.Get(Spec.meleeHitFx);

				hitFxPool.TrySpawn(rae.Target.Position.ConvertToVector3(), out _);

				var slashFxPool = GameObjectPool.Get(Spec.meleeFx);

				var dir = (rae.Target.Position - _character.Position).normalized;

				slashFxPool.TrySpawn((_character.Position + dir * 1.0f).ConvertToVector3(), out var slashFx);

				var v3Dir = dir.ConvertToVector3();
				var rotation = Quaternion.FromToRotation(Vector3.right, v3Dir);

				slashFx.transform.rotation = Quaternion.Euler(0, 0, rotation.eulerAngles.y + rotation.eulerAngles.z);

				// FIXME : 여기서 공격 적합성 검사를 하고 데미지를 입혀야 함 (공격 범위 안에 들어왔는지)
				CoreService.Event.SendImmediate(rae.Target,
					DamageEvent.Create(_character, _character.AttackDamage));
			}
		}

		public void UpdateFrame(float dt)
		{
			if (_currentState == State.Attacking)
			{
				_timePassed += dt;

				if (_timePassed >= Spec.meleeDuration / _character.AttackSpeed)
				{
					_timePassed -= Spec.meleeDuration / _character.AttackSpeed;
					_currentState = State.Cooling;
				}
			}
			else if (_currentState == State.Cooling)
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