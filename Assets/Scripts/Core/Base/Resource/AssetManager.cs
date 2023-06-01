using System;
using System.Collections.Generic;
using Core.Interface;
using UnityEngine;

namespace Core.Base.Resource
{
	public class AssetManager : IAssetManager
	{
		private readonly Dictionary<Type, Dictionary<string, UnityEngine.Object>> _resources = new();

		public void Load(IAssetSpecs[] assetSpec)
		{
			foreach (var assetSpecs in assetSpec)
			{
				var resourcesHolder = new Dictionary<string, UnityEngine.Object>();

				foreach (var keyToPath in assetSpecs.KeyToPath)
				{
					resourcesHolder.Add(keyToPath.Key, Resources.Load(keyToPath.Value));
				}

				_resources.Add(assetSpecs.AssetType, resourcesHolder);

				assetSpecs.Dispose();
			}
		}

		public bool TryGet<T>(string key, out T result) where T : UnityEngine.Object
		{
			var type = typeof(T);
			result = null;

			if (_resources.TryGetValue(type, out var resHolder) &&
			    resHolder.TryGetValue(key, out var asset))
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