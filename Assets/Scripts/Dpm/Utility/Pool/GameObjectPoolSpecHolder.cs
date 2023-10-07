using System;
using UnityEngine;

namespace Dpm.Utility.Pool
{
	[Serializable]
	public struct GameObjectPoolSpec : IGameSpec
	{
		/// <summary>
		/// 스펙명
		/// </summary>
		[SerializeField]
		private string name;

		public string Name => name;

		/// <summary>
		/// Prefab의 스펙명
		/// </summary>
		public string prefabSpecName;

		/// <summary>
		/// 동시에 사용할 수 있는 오브젝트의 최대 개수. 0이면 무제한
		/// </summary>
		public int maxCount;

		/// <summary>
		/// Spawn된 이후 Deactivate될 때까지의 수명. 0이면 무제한
		/// </summary>
		public float lifeTime;
	}

	[CreateAssetMenu(menuName = "GameObjectPoolSpec", fileName = "GameObjectPoolSpec.asset")]
	public class GameObjectPoolSpecHolder : SpecsHolderBase<GameObjectPoolSpec>
	{
	}
}