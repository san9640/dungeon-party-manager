using System;
using System.Collections.Generic;
using Core.Interface;
using UnityEditor;
using UnityEngine;

namespace Core.Base.Resource
{
	public class AssetManager : IAssetManager
	{
		private readonly Dictionary<Type, Dictionary<string, UnityEngine.Object>> _resources = new();

		public AssetManager(params string[] assetSpecsHolderPaths)
		{
			foreach (var path in assetSpecsHolderPaths)
			{
				var assetSpecsHolder = AssetDatabase.LoadAssetAtPath<AssetSpecsHolder>(path);

				// 타입 맞춰서 미리 로드해둠
				Load(assetSpecsHolder);
			}
		}

		private void Load(AssetSpecsHolder assetSpecsHolder)
		{
			var resourcesHolder = new Dictionary<string, UnityEngine.Object>();

			foreach (var assetSpec in assetSpecsHolder.specs)
			{
				var resource = Resources.Load(assetSpec.path);

				resourcesHolder.Add(assetSpec.name, resource);
			}

			_resources.Add(assetSpecsHolder.AssetType, resourcesHolder);
		}

		public bool TryGet<T>(string specName, out T result) where T : UnityEngine.Object
		{
			var type = typeof(T);
			result = null;

			if (_resources.TryGetValue(type, out var resHolder) &&
			    resHolder.TryGetValue(specName, out var asset))
			{
				result = asset as T;

				return true;
			}

			return false;
		}

		public void Dispose()
		{
			_resources.Clear();
		}
	}
}