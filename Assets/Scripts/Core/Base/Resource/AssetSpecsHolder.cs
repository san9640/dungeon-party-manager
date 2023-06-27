using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Core.Base.Resource
{
	public enum AssetTypes
	{
		Prefab,
		Texture,
		ScriptableObject,
	}

	[Serializable]
	public struct AssetSpec
	{
		public string name;
		public string path;
	}

	[CreateAssetMenu(menuName = "AssetSpecsHolder", fileName = "NewAssetSpecsHolder.asset")]
	public class AssetSpecsHolder : ScriptableObject
	{
		public AssetTypes type;
		public List<AssetSpec> specs = new();

		public Type AssetType =>
			type switch
			{
				AssetTypes.Prefab => typeof(GameObject),
				AssetTypes.Texture => typeof(Texture2D),
				AssetTypes.ScriptableObject => typeof(ScriptableObject),
				_ => typeof(UnityEngine.Object)
			};
	}
}