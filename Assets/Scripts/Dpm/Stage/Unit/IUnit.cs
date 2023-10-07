using System;
using Core.Interface;
using Dpm.Stage.Physics;
using Dpm.Utility.State;
using UnityEngine;

namespace Dpm.Stage.Unit
{

	public interface IUnit : ICustomCollider, IEventListener
	{
		int Id { get; }

		string Name { get; }

		UnitRegion Region { get; }

		Vector2 Position { get; set; }

		IState CurrentState { get; }
	}
}