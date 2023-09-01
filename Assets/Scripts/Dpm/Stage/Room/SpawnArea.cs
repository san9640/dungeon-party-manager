using Dpm.Utility;
using Dpm.Utility.Constants;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dpm.Stage.Room
{
	public class SpawnArea : MonoBehaviour
	{
		[SerializeField]
		private Vector2 minOffset;

		[SerializeField]
		private Vector2 maxOffset;

		[SerializeField]
		private Direction direction;

		public Vector2 Min
		{
			get
			{
				var pivot = new Vector2(transform.position.x, transform.position.y);

				return pivot + minOffset;
			}
		}

		public Vector2 Max
		{
			get
			{
				var pivot = new Vector2(transform.position.x, transform.position.y);

				return pivot + maxOffset;
			}
		}

		public Direction Direction => direction;

		/// <summary>
		/// TODO : 테스트용 코드이므로 지워야 함
		/// </summary>
		public Vector2 RandomPos
		{
			get
			{
				var x = Random.Range(Min.x, Max.x);
				var y = Random.Range(Min.y, Max.y);

				return new Vector2(x, y);
			}
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;

			var min = transform.position + new Vector3(minOffset.x, minOffset.y, 0);
			var max = transform.position + new Vector3(maxOffset.x, maxOffset.y, 0);
			var center = (min + max) / 2;
			var size = max - min;

			Gizmos.DrawWireCube(center, size);
		}
#endif
	}
}