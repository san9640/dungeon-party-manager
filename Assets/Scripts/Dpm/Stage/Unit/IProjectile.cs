using System;
using Dpm.Stage.Physics;
using Dpm.Stage.Render;
using Dpm.Stage.Spec;
using UnityEngine;
using IState = Dpm.Utility.State.IState;

namespace Dpm.Stage.Unit
{
	public enum ProjectileType
	{
		Follow,
		Linear,
	}

	public interface IProjectile : ICustomCollider, IDisposable
	{
		IState CurrentState { get; }

		IUnit Shooter { get; }

		float Speed { get; }

		Vector2 LookDir { set; }
	}
}