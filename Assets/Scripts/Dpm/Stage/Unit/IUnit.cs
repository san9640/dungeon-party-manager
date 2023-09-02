using System;
using Dpm.Stage.Physics;
using UnityEngine;

namespace Dpm.Stage.Unit
{
	[Flags]
	public enum UnitRegion
	{
		None = 0,
		Ally = 1 << 0,
		Enemy = 1 << 1,
		Neutral = 1 << 2,
		Creature = Ally | Enemy,
		All = Ally | Enemy | Neutral,
	}

	public interface IUnit : ICustomCollider
	{
		int Id { get; }

		string Name { get; }

		UnitRegion Region { get; }

		Vector2 Position { get; set; }

		void OnInit();

		void EnterField();

		void ExitField();
	}
}