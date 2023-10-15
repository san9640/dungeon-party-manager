using System;
using UnityEngine;

namespace Dpm.Stage.Physics
{
	/// <summary>
	/// TODO : Pivot 추가
	/// </summary>
	[Serializable]
	public struct Bounds2D
	{
		public Vector2 Min => center - extents;

		public Vector2 Max => center + extents;

		public Vector2 Size
		{
			get => extents * 2;
			set => extents = value * 0.5f;
		}

		public Vector2 extents;

		[HideInInspector]
		public Vector2 center;

		public Bounds2D(Vector2 center, Vector2 size)
		{
			this.center = center;
			extents = size * 0.5f;
		}
	}
}