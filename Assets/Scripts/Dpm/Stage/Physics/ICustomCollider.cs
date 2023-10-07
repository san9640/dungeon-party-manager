using Core.Interface;
using UnityEngine;

namespace Dpm.Stage.Physics
{
	public interface ICustomCollider : IEventListener
	{
		Bounds2D Bounds { get; }

		Vector2 Position { get; set; }

		bool OnSimulateCrash(ICustomCollider other);
	}
}