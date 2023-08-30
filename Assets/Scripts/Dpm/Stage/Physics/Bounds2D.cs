using UnityEngine;

namespace Dpm.Stage.Physics
{
	public struct Bounds2D
	{
		public Vector2 Min => Center - Extents;

		public Vector2 Max => Center + Extents;

		private Vector2 _size;

		public Vector2 Size
		{
			get => _size;
			set
			{
				_size = value;
				_extents = _size * 0.5f;
			}
		}

		private Vector2 _extents;

		public Vector2 Extents
		{
			get => _extents;
			set
			{
				_extents = value;
				_size = _extents * 2;
			}
		}

		public Vector2 Center;

		public Bounds2D(Vector2 center, Vector2 size)
		{
			Center = center;
			_size = size;
			_extents = size * 0.5f;
		}
	}
}