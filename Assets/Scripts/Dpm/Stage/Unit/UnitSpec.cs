using UnityEngine;

namespace Dpm.Stage.Unit
{
	public class UnitSpec : MonoBehaviour
	{
		[SerializeField]
		public Vector2 boundsSize;

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (Application.isPlaying)
			{
				return;
			}

			Gizmos.color = Color.green;

			Gizmos.DrawWireCube(transform.position, boundsSize);
		}
#endif
	}
}