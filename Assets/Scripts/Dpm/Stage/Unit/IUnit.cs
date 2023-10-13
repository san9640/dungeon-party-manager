using System;
using Core.Interface;
using Dpm.Stage.Physics;
using Dpm.Utility.State;
using UnityEngine;

namespace Dpm.Stage.Unit
{

	public interface IUnit : ICustomCollider
	{
		int Id { get; }

		string Name { get; }

		UnitRegion Region { get; }

		IState CurrentState { get; }
	}
}