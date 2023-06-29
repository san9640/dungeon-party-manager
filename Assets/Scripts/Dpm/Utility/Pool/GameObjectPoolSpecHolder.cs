using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dpm.Utility.Pool
{
	[Serializable]
	public struct GameObjectPoolSpec
	{
		/// <summary>
		/// 스펙명
		/// </summary>
		public string name;

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
	public class GameObjectPoolSpecHolder : ScriptableObject
	{
		public List<GameObjectPoolSpec> specs;

		private IReadOnlyDictionary<string, GameObjectPoolSpec> _nameToSpec;

		public IReadOnlyDictionary<string, GameObjectPoolSpec> NameToSpec =>
			_nameToSpec ??= specs.ToDictionary(spec => spec.name);
	}
}