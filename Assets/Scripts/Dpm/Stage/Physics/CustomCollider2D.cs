using System;
using UnityEngine;

namespace Dpm.Stage.Physics
{
	public class CustomCollider2D : MonoBehaviour
	{
		public Bounds2D bounds;

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;

			Gizmos.DrawWireCube(transform.position, bounds.Size);
		}
#endif
	}
}