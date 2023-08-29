using UnityEngine;

namespace Dpm.Stage.Unit
{
	public enum UnitRegion
	{
		Ally,
		Enemy,
		Neutral
	}

	public interface IUnit
	{
		void EnterField();

		void ExitField();
	}
}