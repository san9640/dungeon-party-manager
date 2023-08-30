using Dpm.Stage.Physics;

namespace Dpm.Stage.Unit
{
	public enum UnitRegion
	{
		Ally,
		Enemy,
		Neutral
	}

	public interface IUnit : ICustomCollider
	{
		void EnterField();

		void ExitField();
	}
}